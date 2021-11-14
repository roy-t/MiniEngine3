using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        private record class Group(string Name, int startFace, int endFace);

        private sealed class ParseState
        {
            public List<Vector4> Vertices { get; }
            public List<Vector4> Normals { get; }
            public List<Vector4> Texcoords { get; }
            public List<Point3[]> Faces { get; }
            public List<Group> Groups { get; }

            public List<string> MaterialLibraries { get; }

            public string Object { get; set; }
            public string Material { get; set; }

            public Group? Group { get; set; }

            public ParseState()
            {
                this.Vertices = new List<Vector4>();
                this.Normals = new List<Vector4>();
                this.Texcoords = new List<Vector4>();
                this.Faces = new List<Point3[]>();
                this.Groups = new List<Group>();
                this.MaterialLibraries = new List<string>();

                this.Group = null;
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

            return TransformToModelData(state);
        }

        private class ModelVertexComparer : IEqualityComparer<ModelVertex>
        {
            public bool Equals(ModelVertex x, ModelVertex y)
            {
                return x.Normal.Equals(y.Normal) && x.Position.Equals(y.Position) && x.Texcoord.Equals(y.Texcoord);
            }

            public int GetHashCode([DisallowNull] ModelVertex obj)
            {
                return HashCode.Combine(obj.Normal, obj.Position, obj.Texcoord);
            }
        }

        private static ModelData TransformToModelData(ParseState state)
        {
            if (state.Group != null)
            {
                EndPreviousGroup(state);
            }

            if (state.Groups.Count == 0)
            {
                state.Groups.Add(new Group(state.Object, 0, state.Faces.Count - 1));
            }

            var comparer = new ModelVertexComparer();
            var vertices = new Dictionary<ModelVertex, int>(comparer);
            var primitives = new List<Primitive>();
            var indexList = new List<int>();
            var nextIndex = 0;

            foreach (var group in state.Groups)
            {
                var startIndex = indexList.Count;

                for (var fi = group.startFace; fi <= group.endFace; fi++)
                {
                    var face = state.Faces[fi];

                    if (face.Length != 3)
                    {
                        throw new Exception("Face is not a triangle");
                    }

                    var indices = new int[3];
                    for (var i = 0; i < 3; i++)
                    {
                        var lookup = face[i];

                        var position = state.Vertices[lookup.X - 1];
                        var texcoord = state.Texcoords[lookup.Y - 1];
                        var normal = state.Normals[lookup.Z - 1];

                        var p = new Vector3(position.X, position.Y, position.Z);
                        var t = new Vector2(texcoord.X, texcoord.Y);
                        var n = new Vector3(normal.X, normal.Y, normal.Z);
                        var vertex = new ModelVertex(p, t, n);

                        if (vertices.TryGetValue(vertex, out var index))
                        {
                            indices[i] = index;
                        }
                        else
                        {
                            indices[i] = nextIndex;
                            vertices.Add(vertex, nextIndex);
                            ++nextIndex;
                        }
                    }
                    indexList.AddRange(indices);
                }

                primitives.Add(new Primitive(group.Name, startIndex, indexList.Count - startIndex));
            }

            var vertexArray = new ModelVertex[indexList.Max() + 1];
            foreach (var tuple in vertices)
            {
                vertexArray[tuple.Value] = tuple.Key;
            }

            return new ModelData(state.Object, vertexArray, indexList.ToArray(), primitives.ToArray());
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
            return ParseList(statement, "g", arguments => NewGroup(state, arguments));
        }

        private static void EndPreviousGroup(ParseState state)
        {
            if (state.Group != null)
            {
                state.Groups.Add(new Group(state.Group.Name, state.Group.startFace, state.Faces.Count - 1));
            }
        }

        private static void NewGroup(ParseState state, string[] arguments)
        {
            EndPreviousGroup(state);

            var name = string.Join(' ', arguments);
            state.Group = new Group(name, state.Faces.Count, 0);
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
            return ParseList(statement, "v", s => float.Parse(s), elements => state.Vertices.Add(ToVector(elements)));
        }

        /// <summary>
        /// Specifies a texture vertex and its coordinates. A 1D texture requires only u texture coordinates, a 2D texture requires both u and v texture coordinates, and a 3D texture requires all three coordinates.
        /// syntax: vt u v w
        /// </summary>
        private static bool ParseVertexTexture(ParseState state, string statement)
        {
            return ParseList(statement, "vt", s => float.Parse(s), elements => state.Texcoords.Add(ToVector(elements)));
        }

        /// <summary>
        /// Specifies a normal vector with components i, j, and k.
        /// syntax: vn i j k
        /// </summary>
        private static bool ParseNormal(ParseState state, string statement)
        {
            return ParseList(statement, "vn", s => float.Parse(s), elements => state.Normals.Add(ToVector(elements)));
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

        private static Vector4 ToVector(float[] elements)
        {
            var vector = Vector4.Zero;
            vector.X = elements.Length > 0 ? elements[0] : 0;
            vector.Y = elements.Length > 1 ? elements[1] : 0;
            vector.Z = elements.Length > 2 ? elements[2] : 0;
            vector.W = elements.Length > 3 ? elements[3] : 0;


            return vector;
        }
    }
}
