﻿using System.Numerics;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.NoiseShader;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.Graphics.Lighting.PointLights;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class NoiseGenerator
{
    private readonly Device Device;
    private readonly ConstantBuffer<Constants> ConstantBuffer;
    private readonly NoiseShaderKernel Kernel;

    public NoiseGenerator(Device device, NoiseShaderKernel noiseShader)
    {
        this.Device = device;
        this.ConstantBuffer = new ConstantBuffer<Constants>(device, $"{nameof(NoiseGenerator)}_CB");
        this.Kernel = noiseShader;
    }

    public void Generate()
    {
        var context = this.Device.ImmediateContext;

        var stride = 32u;             
        var vertices = new Vector3[stride * stride];
        var cBuffer = new Constants()
        {
            Stride = stride
        };
        this.ConstantBuffer.MapData(context, cBuffer);
        context.CS.SetConstantBuffer(Constants.Slot, this.ConstantBuffer);

        using var input = new StructuredBuffer<Vector3>(this.Device, "input");
        input.MapData(context, vertices);
        
        using var output = new RWStructuredBuffer<Vector3>(this.Device, "output", vertices.Length);
        
        context.CS.SetShader(this.Kernel);
        context.CS.SetShaderResource(NoiseShader.Tile, input);
        context.CS.SetUnorderedAccessView(NoiseShader.World, output);

        // TODO: https://docs.microsoft.com/en-us/windows/win32/api/d3d11/nf-d3d11-id3d11devicecontext-dispatch ?????

        context.CS.Dispatch(4, 4, 1);

        var data = new Vector3[vertices.Length];
        output.ReadData(context, data);
    }

    private static int GetDispatchSize(int threadGroupSize, int elements)
    {
        return (elements + threadGroupSize - 1) / threadGroupSize;
    }
}