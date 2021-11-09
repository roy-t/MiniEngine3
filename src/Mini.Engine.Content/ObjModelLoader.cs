using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content
{
    /// <summary>
    /// Specification: http://www.martinreddy.net/gfx/3d/OBJ.spec
    /// </summary>
    public sealed class ObjModelLoader : IModelLoader
    {
        private sealed class ParseState
        {
            public string Group { get; set; }
            public string Object { get; set; }
            public string Material { get; set; }
        }


        public ModelData Load(IVirtualFileSystem fileSystem, string fileName)
        {



            throw new System.NotImplementedException();
        }

        #region General Statements
        private bool ParseComment(string line)
        {
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
        private bool ParseGroup(string line)
        {

        }

        /// <summary>
        /// Object name statements let you assign a name to an entire object in a single file.
        /// syntax: o object_name
        /// </summary>
        private bool ParseObject(string line)
        {

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
        private bool ParseVertex(string line)
        {

        }

        /// <summary>
        /// Specifies a texture vertex and its coordinates. A 1D texture requires only u texture coordinates, a 2D texture requires both u and v texture coordinates, and a 3D texture requires all three coordinates.
        /// syntax: vt u v w
        /// </summary>
        private bool ParseVertexTexture(string line)
        {

        }

        /// <summary>
        /// Specifies a normal vector with components i, j, and k.
        /// syntax: vn i j k
        /// </summary>
        private bool ParseNormal(string line)
        {

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
        private bool ParseFace(string line)
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
        private bool ParseUseMtlLib(string line)
        {

        }


        /// <summary>
        /// Specifies the material name for the element following it. Once a material is assigned, it cannot be turned off; it can only be changed.
        /// syntax: usemtl material_name
        /// </summary>
        private bool ParseUseMtl(string line)
        {

        }
        #endregion
    }
}
