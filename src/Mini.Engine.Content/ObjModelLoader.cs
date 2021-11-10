using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Serilog;
using Vortice.Mathematics;

namespace Mini.Engine.Content
{
    /// <summary>
    /// Specification: http://www.martinreddy.net/gfx/3d/OBJ.spec
    /// </summary>
    public sealed class ObjModelLoader : IModelLoader
    {
        private sealed class ParseState
        {
            public List<Vector3> Vertices { get; }
            public List<Vector3> Normals { get; }
            public List<Vector2> Texcoords { get; }
            public List<Point3[]> Faces { get; }
            public List<Primitive> Primitives { get; }

            public List<string> MaterialLibraries { get; }

            public string Group { get; set; }
            public string Object { get; set; }
            public string Material { get; set; }

            public ParseState()
            {
                this.Vertices = new List<Vector3>();
                this.Normals = new List<Vector3>();
                this.Texcoords = new List<Vector2>();
                this.Faces = new List<Point3[]>();
                this.Primitives = new List<Primitive>();
                this.MaterialLibraries = new List<string>();

                this.Group = "default";
                this.Object = string.Empty;
                this.Material = string.Empty;
            }
        }
        private const StringSplitOptions SplitOptions = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
        private readonly ILogger Logger;

        public ObjModelLoader(ILogger logger)
        {
            this.Logger = logger.ForContext<ObjModelLoader>();
        }

        public ModelData Load(IVirtualFileSystem fileSystem, string fileName)
        {
            var lines = fileSystem.ReadAllLines(fileName);
            var state = new ParseState();
            foreach (var line in lines)
            {
                var statement = line.Trim().ToLowerInvariant();
                var parsed =
                    ParseVertex(state, statement) ||
                    ParseVertexTexture(state, statement) ||
                    ParseNormal(state, statement) ||
                    ParseFace(state, statement) ||
                    ParseGroup(state, statement) ||
                    ParseObject(state, statement) ||
                    ParseUseMtl(state, statement) ||
                    ParseComment(state, statement) ||
                    ParseUseMtlLib(state, statement) ||
                    false;

                if (!parsed)
                {
                    this.Logger.Warning("Unknown statement: {@line}", statement);
                }
            }

            var vertices = new ModelVertex[state.Vertices.Count];
            var indices = new int[state.Faces.Count * 3];
            var primitives = new List<Primitive>();

            // TODO: almost works but there's a small bug that makes me miss some triangles
            for (var i = 0; i < state.Faces.Count; i++)
            {
                var face = state.Faces[i];
                if (face.Length == 3)
                {
                    var index = new int[3];
                    for (var j = 0; j < 3; j++)
                    {
                        var point = face[j];
                        var position = state.Vertices[point.X - 1];
                        var texcoord = state.Texcoords[point.Y - 1];
                        var normal = state.Normals[point.Z - 1];
                        vertices[point.X - 1] = new ModelVertex(position, texcoord, normal);

                        index[j] = point.X - 1;
                    }

                    indices[i * 3 + 0] = index[0];
                    indices[i * 3 + 1] = index[1];
                    indices[i * 3 + 2] = index[2];
                }
                else
                {
                    throw new Exception("Unexpected number of elements per face");
                }
            }

            // TODO: support models with more than one primitive!
            primitives.Add(new Primitive(state.Object, 0, indices.Length / 3));
            return new ModelData(vertices, indices, primitives.ToArray());
        }

        #region General Statements
        private static bool ParseComment(ParseState _, string statement)
        {
            return ParseList(statement, "#", _ => { });
        }
        #endregion

        #region Grouping Statements
        // All grouping statements are state-setting.This means that once a
        // group statement is set, it applies to all elements that follow
        // until the next group statement.

        /// <summary>
        /// Group name statements are used to organize collections of elements and simplify data manipulation for operations in model.
        /// syntax: g group_name1 group_name2 ...
        /// </summary>
        private static bool ParseGroup(ParseState state, string statement)
        {
            return ParseList(statement, "g", arguments => state.Group = string.Join(' ', arguments));
            // TODO: start a new group and end previous groups
        }

        /// <summary>
        /// Object name statements let you assign a name to an entire object in a single file.
        /// syntax: o object_name
        /// </summary>
        private static bool ParseObject(ParseState state, string statement)
        {
            return Parse(statement, "o", obj => state.Object = obj);
        }
        #endregion


        #region Vertex Data
        // The vertex data is represented by four vertex lists; one for each type
        // of vertex coordinate.A right-hand coordinate system is used to specify
        // the coordinate locations.

        /// <summary>
        /// Specifies a geometric vertex and its x y z coordinates. Rational curves and surfaces require a fourth homogeneous coordinate, also called the weight.
        /// syntax: v x y z w
        /// </summary>
        private static bool ParseVertex(ParseState state, string statement)
        {
            return ParseList(statement, "v", s => float.Parse(s), elements => state.Vertices.Add(ToVector3(elements)));
        }

        /// <summary>
        /// Specifies a texture vertex and its coordinates. A 1D texture requires only u texture coordinates, a 2D texture requires both u and v texture coordinates, and a 3D texture requires all three coordinates.
        /// syntax: vt u v w
        /// </summary>
        private static bool ParseVertexTexture(ParseState state, string statement)
        {
            return ParseList(statement, "vt", s => float.Parse(s), elements => state.Texcoords.Add(ToVector2(elements)));
        }

        /// <summary>
        /// Specifies a normal vector with components i, j, and k.
        /// syntax: vn i j k
        /// </summary>
        private static bool ParseNormal(ParseState state, string statement)
        {
            return ParseList(statement, "vn", s => float.Parse(s), elements => state.Normals.Add(ToVector3(elements)));
        }

        #endregion
        #region Referencing Vertex Data
        // For all elements, reference numbers are used to identify geometric
        // vertices, texture vertices, vertex normals, and parameter space
        // vertices.

        // Each of these types of vertices is numbered separately, starting with
        // 1. This means that the first geometric vertex in the file is 1, the
        // second is 2, and so on.The first texture vertex in the file is 1, the
        // second is 2, and so on.The numbering continues sequentially throughout
        // the entire file.Frequently, files have multiple lists of vertex data.
        // This numbering sequence continues even when vertex data is separated by
        // other data.

        // In addition to counting vertices down from the top of the first list in
        // the file, you can also count vertices back up the list from an
        // element's position in the file. When you count up the list from an
        // element, the reference numbers are negative.A reference number of -1
        // indicates the vertex immediately above the element.A reference number
        // of -2 indicates two references above and so on.

        /// <summary>
        /// Specifies a face element and its vertex reference number. You can optionally include the texture vertex and vertex normal reference numbers.
        /// syntax: f v1/vt1/vn1 v2/vt2/vn2 v3/vt3/vn3..  or f v1//vn1 v2//vn2 v3//vn3  or f v1/vt1 v2/vt2 v3/vt3
        /// </summary>
        private static bool ParseFace(ParseState state, string statement)
        {
            return ParseList(statement, "f", s => ParseTriplet(s), triplet => state.Faces.Add(triplet));
        }

        #endregion
        #region Display/render attributes
        // Display and render attributes describe how an object looks when
        // displayed in Model and PreView or when rendered with Image.

        /// <summary>
        /// Specifies the material library file for the material definitions set with the usemtl statement.You can specify multiple filenames with mtllib.If multiple filenames are specified, the first file listed is searched first for the material definition, the second file is searched next, and so on.
        /// syntax: mtllib filename1 filename2...
        /// </summary>
        private static bool ParseUseMtlLib(ParseState state, string statement)
        {
            return Parse(statement, "mtllib", library => state.MaterialLibraries.Add(library));
        }


        /// <summary>
        /// Specifies the material name for the element following it. Once a material is assigned, it cannot be turned off; it can only be changed.
        /// syntax: usemtl material_name
        /// </summary>
        private static bool ParseUseMtl(ParseState state, string statement)
        {
            return Parse(statement, "usemtl", material => state.Material = material);
        }
        #endregion

        private static bool Parse(string statement, string instruction, Action<string> stateChange)
        {
            return ParseList(statement, instruction, s => s, list => stateChange(list.Single()));
        }

        private static bool ParseList(string statement, string instruction, Action<string[]> stateChange)
        {
            return ParseList(statement, instruction, s => s, stateChange);
        }

        private static bool Parse<T>(string statement, string instruction, Func<string, T> converter, Action<T> stateChange)
        {
            return ParseList(statement, instruction, converter, list => stateChange(list.Single()));
        }

        private static bool ParseList<T>(string statement, string instruction, Func<string, T> converter, Action<T[]> stateChange)
        {
            if (statement.StartsWith($"{instruction} "))
            {
                var tokens = statement[instruction.Length..].Split(Array.Empty<char>(), SplitOptions);
                var arguments = new T[tokens.Length];
                for (var i = 0; i < tokens.Length; i++)
                {
                    arguments[i] = converter(tokens[i]);
                }
                stateChange(arguments);
                return true;
            }

            return false;
        }

        private static Point3 ParseTriplet(string token)
        {
            var elements = ParseGroup(token, 3);
            return new Point3(int.Parse(elements[0]), int.Parse(elements[1]), int.Parse(elements[2]));
        }

        private static string[] ParseGroup(string token, int expectedElements)
        {
            var elements = token.Split('/', SplitOptions);
            if (elements.Length != expectedElements)
            {
                throw new Exception($"Could not split token '{token}' into a group of {expectedElements} elements");
            }

            return elements;
        }

        private static Vector2 ToVector2(float[] elements)
        {
            if (elements.Length != 2)
            {
                throw new ArgumentException($"Could not convert an array with {elements.Length} elements to a Vector2");
            }
            return new Vector2(elements[0], elements[1]);
        }

        private static Vector3 ToVector3(float[] elements)
        {
            if (elements.Length != 3)
            {
                throw new ArgumentException($"Could not convert an array with {elements.Length} elements to a Vector3");
            }
            return new Vector3(elements[0], elements[1], elements[2]);
        }

    }
}
