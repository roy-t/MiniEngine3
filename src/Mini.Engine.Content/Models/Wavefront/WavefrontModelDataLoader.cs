using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Mini.Engine.Content.Materials;
using Mini.Engine.Content.Models.Wavefront.Objects;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources;
using Mini.Engine.IO;
using Vortice.Mathematics;

namespace Mini.Engine.Content.Models.Wavefront;

/// <summary>
/// Specification: http://www.martinreddy.net/gfx/3d/OBJ.spec
/// </summary>
internal sealed partial class WavefrontModelDataLoader : IContentDataLoader<ModelData>
{
    private readonly ObjStatementParser[] Parsers;
    private readonly IVirtualFileSystem FileSystem;
    private readonly IContentLoader<MaterialContent> MaterialLoader;

    public WavefrontModelDataLoader(IVirtualFileSystem fileSystem, IContentLoader<MaterialContent> materialLoader)
    {
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
        this.MaterialLoader = materialLoader;
    }

    public ModelData Load(Device device, ContentId id, ILoaderSettings settings)
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

        if (state.Group != null)
        {
            state.EndPreviousGroup();
        }

        if (state.Groups.Count == 0)
        {
            state.Groups.Add(new Group(state.Object, 0, state.Faces.Count - 1) { Material = state.Material });
        }

        return this.TransformToModelData(device, id, state, settings);
    }

    private ModelData TransformToModelData(Device device, ContentId id, ParseState state, ILoaderSettings settings)
    {
        var materials = this.LoadMaterialData(device, id, state, settings);

        var vertices = new List<ModelVertex>(state.Positions.Count);
        var indices = new List<int>(state.Faces.Count * 3);
        var primitives = new List<Primitive>(state.Groups.Count);
        var faces = new List<int[]>(state.Faces.Count);

        var indexLookUp = new Dictionary<ModelVertex, int>(new ModelVertexComparer());

        var indexBuffer = new int[4];

        // Wavefront defines positions, normals and texture coordinates separately.
        // The same position point can be used with different normal or texture data.
        // So we first need to identify all faces and find all unique vertices in the process.
        for (var f = 0; f < state.Faces.Count; f++)
        {
            var face = state.Faces[f];

            for (var i = 0; i < face.Length; i++)
            {
                var lookup = face[i];
                var position = state.Positions[lookup.X - 1];
                var texcoord = state.Texcoords[lookup.Y - 1];
                var normal = state.Normals[lookup.Z - 1];

                var p = new Vector3(position.X, position.Y, position.Z);
                // obj texture coordinates use {0, 0} as top left
                var t = new Vector2(texcoord.X, 1.0f - texcoord.Y);
                var n = new Vector3(normal.X, normal.Y, normal.Z);
                var vertex = new ModelVertex(p, t, n);

                if (indexLookUp.TryGetValue(vertex, out var index))
                {
                    indexBuffer[i] = index;
                }
                else
                {
                    indexBuffer[i] = vertices.Count;
                    indexLookUp.Add(vertex, vertices.Count);
                    vertices.Add(vertex);
                }
            }

            if (face.Length == 3)
            {
                faces.Add(new int[] { indexBuffer[0], indexBuffer[1], indexBuffer[2] });
            }
            else if (face.Length == 4)
            {
                faces.Add(new int[] { indexBuffer[0], indexBuffer[1], indexBuffer[2], indexBuffer[3] });
            }
            else
            {
                throw new Exception($"Face is not a triangle or quad but a polygon with {face.Length} vertices");
            }
        }

        for (var g = 0; g < state.Groups.Count; g++)
        {
            var group = state.Groups[g];
            var startIndex = indices.Count;

            for (var f = group.StartFace; f <= group.EndFace; f++)
            {
                var face = faces[f];
                if (face.Length == 3)
                {
                    indices.Add(face[2]);
                    indices.Add(face[1]);
                    indices.Add(face[0]);
                }
                else if (face.Length == 4)
                {
                    indices.Add(face[2]);
                    indices.Add(face[1]);
                    indices.Add(face[0]);

                    indices.Add(face[0]);
                    indices.Add(face[3]);
                    indices.Add(face[2]);
                }
            }

            var materialIndex = GetMaterialIdForGroup(materials, group);
            var indexCount = indices.Count - startIndex;

            var primitiveBounds = ComputeBounds(indices, vertices, startIndex, indexCount);
            primitives.Add(new Primitive(group.Name, primitiveBounds, materialIndex, startIndex, indexCount));
        }

        var modelBounds = ComputeBounds(primitives);
        return new ModelData(id, modelBounds, vertices.ToArray(), indices.ToArray(), primitives.ToArray(), materials);
    }

    private static BoundingBox ComputeBounds(List<int> indices, List<ModelVertex> vertices, int startIndex, int indexCount)
    {
        var positions = new Vector3[indexCount];
        for (var i = 0; i < indexCount; i++)
        {
            positions[i] = vertices[indices[startIndex + i]].Position;
        }
        return BoundingBox.CreateFromPoints(positions);
    }

    private static BoundingBox ComputeBounds(IReadOnlyList<Primitive> primitives)
    {
        var bounds = primitives[0].Bounds;
        for (var i = 1; i < primitives.Count; i++)
        {
            var primitive = primitives[i];
            bounds = BoundingBox.CreateMerged(bounds, primitive.Bounds);
        }

        return bounds;
    }

    private MaterialContent[] LoadMaterialData(Device device, ContentId id, ParseState state, ILoaderSettings settings)
    {
        var materialKeys = state.Groups.Select(x => x.Material ?? string.Empty).ToHashSet().ToArray();
        var materials = new MaterialContent[materialKeys.Length];
        for (var i = 0; i < materials.Length; i++)
        {
            var materialId = id.RelativeTo(state.MaterialLibrary, materialKeys[i]);
            materials[i] = this.MaterialLoader.Load(device, materialId, ((ModelLoaderSettings)settings).MaterialSettings);
        }

        return materials;
    }

    private static int GetMaterialIdForGroup(MaterialContent[] materials, Group group)
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
