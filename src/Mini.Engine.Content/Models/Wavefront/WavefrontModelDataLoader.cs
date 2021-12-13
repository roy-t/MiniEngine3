using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Materials.Wavefront;
using Mini.Engine.Content.Models.Wavefront.Objects;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models.Wavefront;

/// <summary>
/// Specification: http://www.martinreddy.net/gfx/3d/OBJ.spec
/// </summary>
internal sealed class WavefrontModelDataLoader : IContentDataLoader<ModelData>
{
    private readonly WavefrontMaterialDataLoader MaterialDataLoader;
    private readonly ObjStatementParser[] Parsers;
    private readonly IVirtualFileSystem FileSystem;

    public WavefrontModelDataLoader(IVirtualFileSystem fileSystem)
    {
        this.MaterialDataLoader = new WavefrontMaterialDataLoader(fileSystem);
        this.Parsers = new ObjStatementParser[]
        {
                new VertexPositionParser(),
                new VertexTextureParser(),
                new VertexNormalParser(),

                new FaceParser(),

                new GroupParser(),
                new ObjectParser(),
                new CommentParser(),

                new MtlLibParser(),
                new UseMtlParser()
        };
        this.FileSystem = fileSystem;
    }

    public ModelData Load(ContentId id)
    {
        var text = this.FileSystem.ReadAllText(id.Path).AsSpan();

        var state = new ParseState();
        foreach (var line in text.EnumerateLines())
        {
            foreach (var parser in this.Parsers)
            {
                if (parser.Parse(state, line, this.FileSystem))
                {
                    break;
                }
            }
        }

        return TransformToModelData(id, state);
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

    private ModelData TransformToModelData(ContentId id, ParseState state)
    {
        if (state.Group != null)
        {
            state.EndPreviousGroup();
        }

        if (state.Groups.Count == 0)
        {
            state.Groups.Add(new Group(state.Object, 0, state.Faces.Count - 1));
        }

        var comparer = new ModelVertexComparer();
        var vertices = new Dictionary<ModelVertex, int>(comparer);
        var primitives = new List<Primitive>();
        var materials = this.LoadMaterialData(id, state);
        var indexList = new List<int>();
        var nextIndex = 0;

        foreach (var group in state.Groups)
        {
            var startIndex = indexList.Count;

            for (var fi = group.StartFace; fi <= group.EndFace; fi++)
            {
                var face = state.Faces[fi];

                var indices = new int[face.Length];
                for (var i = 0; i < face.Length; i++)
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

                if (face.Length == 3)
                {
                    indexList.AddRange(indices);
                }
                else if (face.Length == 4)
                {
                    indexList.AddRange(indices[0..3]);
                    indexList.Add(indices[2]);
                    indexList.Add(indices[3]);
                    indexList.Add(indices[0]);
                }
                else
                {
                    throw new Exception($"Face is not a triangle or quad but a polygon with {face.Length} vertices");
                }
            }

            var materialIndex = GetMaterialIdForGroup(materials, group);
            primitives.Add(new Primitive(group.Name, materialIndex, startIndex, indexList.Count - startIndex));
        }

        var vertexArray = new ModelVertex[indexList.Max() + 1];
        foreach (var tuple in vertices)
        {
            vertexArray[tuple.Value] = tuple.Key;
        }

        var model = new ModelData(id.ToString(), vertexArray, indexList.ToArray(), primitives.ToArray(), materials);
        TextureLookup.MakeTexturesPathsRelativeToContentPath(model);
        TextureLookup.AddFallbackTextures(model);

        return model;
    }

    private MaterialData[] LoadMaterialData(ContentId id, ParseState state)
    {
        var materialKeys = state.Groups.Select(x => x.Material ?? string.Empty).ToHashSet().ToArray();
        var materials = new MaterialData[materialKeys.Length];
        for (var i = 0; i < materials.Length; i++)
        {
            var materialId = id.RelativeTo(state.MaterialLibrary, materialKeys[i]);
            materials[i] = this.MaterialDataLoader.Load(materialId);
        }

        return materials;
    }

    private static int GetMaterialIdForGroup(MaterialData[] materials, Group group)
    {
        var materialIndex = -1;
        for (var i = 0; i < materials.Length; i++)
        {
            if (materials[i].Id.Key == group.Material)
            {
                materialIndex = i;
            }
        }

        if (materialIndex == -1) { throw new KeyNotFoundException($"Material with key ${group.Material} not found"); }

        return materialIndex;
    }
}
