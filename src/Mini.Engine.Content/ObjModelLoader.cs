using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Serilog;

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

            throw new NotImplementedException();
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
            return Parse(statement, "v", s => ParseVector3(s), vertex => state.Vertices.Add(vertex));
        }

        /// <summary>
        /// Specifies a texture vertex and its coordinates. A 1D texture requires only u texture coordinates, a 2D texture requires both u and v texture coordinates, and a 3D texture requires all three coordinates.
        /// syntax: vt u v w
        /// </summary>
        private static bool ParseVertexTexture(ParseState state, string statement)
        {
            return Parse(statement, "vt", s => ParseVector2(s), texcoord => state.Texcoords.Add(texcoord));
        }

        /// <summary>
        /// Specifies a normal vector with components i, j, and k.
        /// syntax: vn i j k
        /// </summary>
        private static bool ParseNormal(ParseState state, string statement)
        {
            return Parse(statement, "n", s => ParseVector3(s), normal => state.Normals.Add(normal));
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

        }


        /// <summary>
        /// Specifies the material name for the element following it. Once a material is assigned, it cannot be turned off; it can only be changed.
        /// syntax: usemtl material_name
        /// </summary>
        private static bool ParseUseMtl(ParseState state, string statement)
        {

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
            if (statement.StartsWith(instruction))
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

        private static Vector2 ParseVector2(string token)
        {
            var elements = token.Split('/', SplitOptions);
            if (elements.Length != 2)
            {
                throw new Exception($"Could not parse token '{token}' to a Vector2");
            }

            return new Vector2(float.Parse(elements[0]), float.Parse(elements[1]));
        }

        private static Vector3 ParseVector3(string token)
        {
            var elements = token.Split('/', SplitOptions);
            if (elements.Length != 3)
            {
                throw new Exception($"Could not parse token '{token}' to a Vector3");
            }

            return new Vector3(float.Parse(elements[0]), float.Parse(elements[1]), float.Parse(elements[2]));
        }
    }
}
