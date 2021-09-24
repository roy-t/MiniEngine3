using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Mini.Engine.DirectX
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ModelVertex
    {
        public Vector3 Position;

        public ModelVertex(Vector3 position)
        {
            this.Position = position;
        }

        public static readonly InputElementDescription[] Elements = new InputElementDescription[]
        {
            new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0)
        };
    }

    public readonly record struct Range(int Offset, int Count);

    public sealed class ModelData
    {
        public readonly ModelVertex[] Vertices;
        public readonly int[] Indices;
        public readonly Range[] Primitives;

        public ModelData()
        {
            var e = 1;
            var z = -5;
            this.Vertices = new ModelVertex[]
            {
                new ModelVertex(new Vector3(-e, 0, z)),
                new ModelVertex(new Vector3(0, e, z)),
                new ModelVertex(new Vector3(e, 0, z)),
            };

            this.Indices = new int[]
            {
                0, 1, 2,
            };

            this.Primitives = new Range[]
            {
                new Range(0, 3),
            };
        }
    }

    public sealed class Model : IDisposable
    {
        public readonly VertexBuffer<ModelVertex> Vertices;
        public readonly IndexBuffer<int> Indices;
        public readonly Range[] Primitives;

        public Model(Device device, ModelData data)
        {
            this.Vertices = new VertexBuffer<ModelVertex>(device);
            this.Indices = new IndexBuffer<int>(device);
            this.Primitives = data.Primitives;
            
            this.Vertices.MapData(device.ImmediateContext, data.Vertices);
            this.Indices.MapData(device.ImmediateContext, data.Indices);
        }

        public int PrimitiveCount => this.Primitives.Length;

        public void Dispose()
        {
            this.Indices.Dispose();
            this.Vertices.Dispose();
        }

        // TODO add a hierarchy of matrices
    }
}
