﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Contexts.States;
using Mini.Engine.DirectX.Resources.Models;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.Graphics.Cameras;
using Mini.Engine.Graphics.Transforms;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Shader = Mini.Engine.Content.Shaders.Generated.Line;

namespace Mini.Engine.Graphics.Diesel;

[Service]
public sealed class LineRenderService : IDisposable
{
    private readonly Shader Shader;
    private readonly Shader.User User;
    private readonly InputLayout InputLayout;

    private readonly RasterizerState Line;
    private readonly DepthStencilState ReverseZ;
    private readonly BlendState Opaque;

    public LineRenderService(Device device, Shader shader)
    {
        this.Line = device.RasterizerStates.Line;
        this.ReverseZ = device.DepthStencilStates.ReverseZ;
        this.Opaque = device.BlendStates.Opaque;

        this.Shader = shader;
        this.User = shader.CreateUserFor<LineRenderService>();
        this.InputLayout = shader.CreateInputLayoutForVs(LineMesh.Elements);
    }

    public void Setup(DeviceContext context, RenderTarget albedo, DepthStencilBuffer depth, int x, int y, int width, int height)
    {
        context.OM.SetRenderTarget(albedo, depth);

        context.Setup(this.InputLayout, Vortice.Direct3D.PrimitiveTopology.LineStrip, this.Shader.Vs, this.Line, x, y, width, height, this.Shader.Ps, this.Opaque, this.ReverseZ);

        context.VS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetConstantBuffer(Shader.ConstantsSlot, this.User.ConstantsBuffer);
    }

    public void Render(DeviceContext context, in PerspectiveCamera camera, in Transform cameraTransform, in LineComponent line, in TransformComponent transform)
    {
        var viewProjection = camera.GetInfiniteReversedZViewProjection(in cameraTransform);

        var mesh = context.Resources.Get(line.Mesh);
        var world = transform.Current.GetMatrix();

        context.IA.SetVertexBuffer(mesh.Vertices);

        this.User.MapConstants(context, world * viewProjection, line.Color);
        context.Draw(mesh.VertexCount);
    }

    public void Dispose()
    {
        this.User.Dispose();
        this.InputLayout.Dispose();
    }
}
