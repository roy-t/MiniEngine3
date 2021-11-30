using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using Mini.Engine.Content.Models.Obj.Models;
using Mini.Engine.DirectX;
using Mini.Engine.IO;
using Serilog;

namespace Mini.Engine.Content.Models.Obj;

/// <summary>
/// Specification: http://www.martinreddy.net/gfx/3d/OBJ.spec
/// </summary>
public sealed class ObjModelLoader : IModelLoader
{
    private readonly ILogger Logger;
    private readonly ObjStatementParser[] Parsers;

    public ObjModelLoader(ILogger logger)
    {
        this.Logger = logger.ForContext<ObjModelLoader>();
        this.Parsers = new ObjStatementParser[]
        {
                new VertexPositionParser(),
                new VertexTextureParser(),
                new VertexNormalParser(),

                new FaceParser(),

                new GroupParser(),
                new ObjectParser(),
                new CommentParser(),

                new MtlLibParser(logger),
                new UseMtlParser()
        };
    }

    public ModelData Load(IVirtualFileSystem fileSystem, string fileName)
    {
        var watch = Stopwatch.StartNew();
        var text = fileSystem.ReadAllText(fileName).AsSpan();

        var state = new ObjParseState(Path.GetDirectoryName(fileName) ?? string.Empty);
        foreach (var line in text.EnumerateLines())
        {
            foreach (var parser in this.Parsers)
            {
                if (parser.Parse(state, line, fileSystem))
                {
                    break;
                }
            }
        }

        this.Logger.Information("Parsing model {@milliseconds}", watch.ElapsedMilliseconds);
        return this.TransformToModelData(state);
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

    private ModelData TransformToModelData(ObjParseState state)
    {
        var watch = Stopwatch.StartNew();
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
                    throw new Exception($"Face is not a triangle or quad but a polygon with ${face.Length} vertices");
                }
            }

            primitives.Add(new Primitive(group.Name, startIndex, indexList.Count - startIndex));
        }

        var vertexArray = new ModelVertex[indexList.Max() + 1];
        foreach (var tuple in vertices)
        {
            vertexArray[tuple.Value] = tuple.Key;
        }

        this.Logger.Information("Transforming model took {@milliseconds}", watch.ElapsedMilliseconds);
        return new ModelData(state.Object, vertexArray, indexList.ToArray(), primitives.ToArray());
    }
}
