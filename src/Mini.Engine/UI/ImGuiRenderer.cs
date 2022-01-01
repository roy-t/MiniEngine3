using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.Content;
using Mini.Engine.Content.Shaders;
using Mini.Engine.Content.Shaders.Simple;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using ImDrawIdx = System.UInt16;

namespace Mini.Engine.UI;

internal sealed class ImGuiRenderer
{
    private int textureCounter;

    private readonly Dictionary<IntPtr, ITexture2D> TextureResources;

    // Borrowed resources
    private readonly Device Device;
    private readonly ImmediateDeviceContext ImmediateContext;

    // Created resources
    private readonly DeferredDeviceContext DeferredContext;
    private readonly SimpleVs VertexShader;
    private readonly SimplePs PixelShader;
    private readonly ITexture2D FontTexture;
    private readonly InputLayout InputLayout;
    private readonly VertexBuffer<ImDrawVert> VertexBuffer;
    private readonly IndexBuffer<ImDrawIdx> IndexBuffer;
    private readonly ConstantBuffer<CBuffer0> ConstantBuffer;

    public ImGuiRenderer(Device device, ContentManager content)
    {
        this.Device = device;
        this.ImmediateContext = device.ImmediateContext;
        this.DeferredContext = device.CreateDeferredContextFor<ImGuiRenderer>();

        this.VertexBuffer = new VertexBuffer<ImDrawVert>(device, "vertices_imgui");
        this.IndexBuffer = new IndexBuffer<ImDrawIdx>(device, "indices_imgui");
        this.ConstantBuffer = new ConstantBuffer<CBuffer0>(device, "constants_imgui");

        this.VertexShader = content.LoadSimpleVs();
        this.PixelShader = content.LoadSimplePs();

        this.InputLayout = this.VertexShader.CreateInputLayout
        (
            device,
            new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 8, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0, InputClassification.PerVertexData, 0)
        );

        this.TextureResources = new Dictionary<IntPtr, ITexture2D>();
        this.FontTexture = CreateFontsTexture(device);

        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.Fonts.TexID = this.RegisterTexture(this.FontTexture);
    }

    public void Render(ImDrawDataPtr data)
    {
        if (data.DisplaySize.X <= 0.0f || data.DisplaySize.Y <= 0.0f || data.TotalVtxCount <= 0)
        {
            return;
        }

        this.VertexBuffer.EnsureCapacity(data.TotalVtxCount, data.TotalVtxCount / 10);
        this.IndexBuffer.EnsureCapacity(data.TotalIdxCount, data.TotalIdxCount / 10);

        var vertexOffset = 0;
        var indexOffset = 0;
        using (var vertexWriter = this.VertexBuffer.OpenWriter(this.DeferredContext))
        using (var indexWriter = this.IndexBuffer.OpenWriter(this.DeferredContext))
        {
            for (var n = 0; n < data.CmdListsCount; n++)
            {
                var cmdlList = data.CmdListsRange[n];
                unsafe
                {
                    vertexWriter.MapData(new Span<ImDrawVert>(cmdlList.VtxBuffer.Data.ToPointer(), cmdlList.VtxBuffer.Size), vertexOffset);
                    vertexOffset += cmdlList.VtxBuffer.Size;

                    indexWriter.MapData(new Span<ImDrawIdx>(cmdlList.IdxBuffer.Data.ToPointer(), cmdlList.IdxBuffer.Size), indexOffset);
                    indexOffset += cmdlList.IdxBuffer.Size;
                }
            }
        }

        // Setup orthographic projection matrix into our constant buffer
        var cBufferData = new CBuffer0() { ProjectionMatrix = Matrix4x4.CreateOrthographicOffCenter(0, data.DisplaySize.X, data.DisplaySize.Y, 0, -1.0f, 1.0f) };
        this.ConstantBuffer.MapData(this.DeferredContext, cBufferData);

        this.SetupRenderState(data, this.DeferredContext);

        // Render command lists
        // (Because we merged all buffers into a single one, we maintain our own offset into them)
        var globalIndexOffset = 0;
        var lobalVertexOffset = 0;
        for (var n = 0; n < data.CmdListsCount; n++)
        {
            var cmdList = data.CmdListsRange[n];
            for (var i = 0; i < cmdList.CmdBuffer.Size; i++)
            {
                var cmd = cmdList.CmdBuffer[i];
                if (cmd.UserCallback != IntPtr.Zero)
                {
                    throw new NotImplementedException("user callbacks not implemented");
                }
                else
                {
                    var left = (int)(cmd.ClipRect.X - data.DisplayPos.X);
                    var top = (int)(cmd.ClipRect.Y - data.DisplayPos.Y);
                    var right = (int)(cmd.ClipRect.Z - data.DisplayPos.X);
                    var bottom = (int)(cmd.ClipRect.W - data.DisplayPos.Y);

                    this.DeferredContext.RS.SetScissorRect(left, top, right - left, bottom - top);

                    if (this.TextureResources.TryGetValue(cmd.TextureId, out var texture))
                    {
                        this.DeferredContext.PS.SetShaderResource(Simple.Texture0, texture);
                    }

                    this.DeferredContext.DrawIndexed((int)cmd.ElemCount, (int)(cmd.IdxOffset + globalIndexOffset), (int)(cmd.VtxOffset + lobalVertexOffset));
                }
            }
            globalIndexOffset += cmdList.IdxBuffer.Size;
            lobalVertexOffset += cmdList.VtxBuffer.Size;
        }

        using var commandList = this.DeferredContext.FinishCommandList();
        this.ImmediateContext.ExecuteCommandList(commandList);
    }

    public void Dispose()
    {
        this.DeferredContext.Dispose();
        this.VertexShader.Dispose();
        this.PixelShader.Dispose();
        this.ConstantBuffer.Dispose();
        this.IndexBuffer.Dispose();
        this.VertexBuffer.Dispose();
        this.FontTexture.Dispose();
        this.InputLayout.Dispose();
    }

    private void SetupRenderState(ImDrawDataPtr drawData, DeferredDeviceContext context)
    {
        context.IA.SetInputLayout(this.InputLayout);
        context.IA.SetVertexBuffer(this.VertexBuffer);
        context.IA.SetIndexBuffer(this.IndexBuffer);
        context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

        context.VS.SetShader(this.VertexShader);
        context.VS.SetConstantBuffer(CBuffer0.Slot, this.ConstantBuffer);

        context.RS.SetViewPort(0, 0, drawData.DisplaySize.X, drawData.DisplaySize.Y);
        context.RS.SetRasterizerState(this.Device.RasterizerStates.CullNone);

        context.PS.SetShader(this.PixelShader);
        context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);

        context.OM.SetRenderTargetToBackBuffer();
        context.OM.SetBlendState(this.Device.BlendStates.NonPreMultiplied);
        context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);
    }

    private IntPtr RegisterTexture(ITexture2D texture)
    {
        var id = (IntPtr)this.textureCounter++;
        this.TextureResources.Add(id, texture);

        return id;
    }

    private static ITexture2D CreateFontsTexture(Device device)
    {
        var io = ImGui.GetIO();
        unsafe
        {
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out var width, out var height);
            var format = Format.R8G8B8A8_UNorm;
            var pixelSpan = new Span<byte>(pixels, width * height * format.SizeOfInBytes());
            return new Texture2D(device, pixelSpan, width, height, format, false, "ImGui_Font");
        }
    }
}
