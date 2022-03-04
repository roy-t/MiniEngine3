using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Mini.Engine.Configuration;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.NoiseShader;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;

namespace Mini.Engine.Graphics.World;

[Service]
public sealed class NoiseGenerator
{
    private readonly Device Device;
    private readonly NoiseShaderKernel NoiseShader;

    public NoiseGenerator(Device device, NoiseShaderKernel noiseShader)
    {
        this.Device = device;
        this.NoiseShader = noiseShader;
    }


    public void Generate()
    {
        var context = this.Device.ImmediateContext;
        var vertices = new Vector3[512];

        using var input = new StructuredBuffer<Vector3>(this.Device, "input");
        using var writer = input.OpenWriter(context);
        writer.MapData(vertices, 0);


        context.CS.SetShaderResource(0, input);

        using var output = new RWStructuredBuffer<Vector3>(this.Device, "output", vertices.Length);
        // WIP WIP WIP WIP WIP WIP see if all the buffers and stuff work?
        // see D:\Projects\C#\MiniRTS\vOld\Engine\MiniEngine.Pipeline.Models\Generators\NoiseGenerator.cs

    }
}
