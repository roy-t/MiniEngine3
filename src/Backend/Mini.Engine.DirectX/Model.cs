using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.InteropServices;
using Mini.Engine.IO;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ModelVertex
    {
        public Vector3 Position;
        public Vector2 Texcoord;
        public Vector3 Normal;

        public ModelVertex(Vector3 position, Vector2 texcoord, Vector3 normal)
        {
            this.Position = position;
            this.Texcoord = texcoord;
            this.Normal = normal;
        }

        public static readonly InputElementDescription[] Elements = new InputElementDescription[]
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0 * sizeof(float), 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 3 * sizeof(float), 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 5 * sizeof(float), 0, InputClassification.PerVertexData, 0)
        };
    }

    public readonly record struct Primitive(int Offset, int Count);
    public sealed record ModelData(ModelVertex[] Vertices, int[] Indices, Primitive[] Primitives);

    public interface IModelLoader
    {
        ModelData Load(IVirtualFileSystem fileSystem, string fileName);
    }

    public sealed class DummyModelLoader : IModelLoader
    {
        public ModelData Load(IVirtualFileSystem fileSystem, string fileName)
        {
            var e = 1;
            var z = -5;
            var vertices = new ModelVertex[]
            {
                new ModelVertex(new Vector3(-e, 0, z), Vector2.Zero, new Vector3(1, 0, 0)),
                new ModelVertex(new Vector3(0, e, z), Vector2.Zero, new Vector3(0, 1, 0)),
                new ModelVertex(new Vector3(e, 0, z), Vector2.Zero, new Vector3(0, 0, 1)),
                new ModelVertex(new Vector3(0, -e, z), Vector2.Zero, new Vector3(1, 1, 1)),
            };

            var indices = new int[]
            {
                0, 1, 2,
                0, 2, 3
            };

            var primitives = new Primitive[]
            {
                new Primitive(0, 3),
                new Primitive(3, 3)
            };

            return new ModelData(vertices, indices, primitives);
        }
    }

    public sealed class Model : IContent
    {
        private readonly IModelLoader Loader;

        public Model(Device device, IVirtualFileSystem fileSystem, IModelLoader loader, string fileName)
        {
            this.Loader = loader;
            this.FileName = fileName;
            this.Reload(device, fileSystem);
        }

        public string FileName { get; }

        public VertexBuffer<ModelVertex> Vertices { get; private set; }
        public IndexBuffer<int> Indices { get; private set; }
        public Primitive[] Primitives { get; private set; }

        public int PrimitiveCount => this.Primitives.Length;

        [MemberNotNull(nameof(Vertices), nameof(Indices), nameof(Primitives))]
        public void Reload(Device device, IVirtualFileSystem fileSystem)
        {
            this.Indices = new IndexBuffer<int>(device);
            this.Vertices = new VertexBuffer<ModelVertex>(device);

            var data = this.Loader.Load(fileSystem, this.FileName);
            this.Primitives = data.Primitives;

            this.Vertices.MapData(device.ImmediateContext, data.Vertices);
            this.Indices.MapData(device.ImmediateContext, data.Indices);
        }

        public void Dispose()
        {
            this.Indices.Dispose();
            this.Vertices.Dispose();
        }
    }
}
