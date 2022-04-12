using System.Numerics;
using ImGuiNET;
using Mini.Engine.Content.Shaders.Generated;
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
    // Borrowed resources
    private readonly Device Device;
    private readonly UITextureRegistry TextureRegistry;
    private readonly ImmediateDeviceContext ImmediateContext;

    // Created resources
    private readonly DeferredDeviceContext DeferredContext;
    private readonly UserInterface Shader;
    private readonly UserInterface.User User;
    private readonly ITexture2D FontTexture;
    private readonly InputLayout InputLayout;
    private readonly VertexBuffer<ImDrawVert> VertexBuffer;
    private readonly IndexBuffer<ImDrawIdx> IndexBuffer;    

    public ImGuiRenderer(Device device, UITextureRegistry textureRegistry, UserInterface shader)
    {
        this.Device = device;
        this.TextureRegistry = textureRegistry;
        this.ImmediateContext = device.ImmediateContext;
        this.DeferredContext = device.CreateDeferredContextFor<ImGuiRenderer>();

        this.VertexBuffer = new VertexBuffer<ImDrawVert>(device, $"{nameof(ImGuiRenderer)}_VB");
        this.IndexBuffer = new IndexBuffer<ImDrawIdx>(device, $"{nameof(ImGuiRenderer)}_IB");

        this.Shader = shader;
        this.User = shader.CreateUserFor<ImGuiRenderer>();

        this.InputLayout = this.Shader.Vs.CreateInputLayout
        (
            device,
            new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 8, 0, InputClassification.PerVertexData, 0),
            new InputElementDescription("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0, InputClassification.PerVertexData, 0)
        );

        this.FontTexture = CreateFontsTexture(device);

        var io = ImGui.GetIO();
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
        io.Fonts.TexID = this.TextureRegistry.Get(this.FontTexture);
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
        this.User.MapConstants(this.DeferredContext, Matrix4x4.CreateOrthographicOffCenter(0, data.DisplaySize.X, data.DisplaySize.Y, 0, -1.0f, 1.0f));

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
                var left = (int)(cmd.ClipRect.X - data.DisplayPos.X);
                var top = (int)(cmd.ClipRect.Y - data.DisplayPos.Y);
                var right = (int)(cmd.ClipRect.Z - data.DisplayPos.X);
                var bottom = (int)(cmd.ClipRect.W - data.DisplayPos.Y);

                this.DeferredContext.RS.SetScissorRect(left, top, right - left, bottom - top);

                var texture = this.TextureRegistry.Get(cmd.TextureId);
                this.DeferredContext.PS.SetShaderResource(UserInterface.Texture, texture);

                this.DeferredContext.DrawIndexed((int)cmd.ElemCount, (int)(cmd.IdxOffset + globalIndexOffset), (int)(cmd.VtxOffset + lobalVertexOffset));
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
        this.IndexBuffer.Dispose();
        this.VertexBuffer.Dispose();
        this.FontTexture.Dispose();
        this.InputLayout.Dispose();
        this.User.Dispose();
    }

    private void SetupRenderState(ImDrawDataPtr drawData, DeferredDeviceContext context)
    {
        context.Setup(this.InputLayout, PrimitiveTopology.TriangleList, this.Shader.Vs, this.Device.RasterizerStates.CullNoneCounterClockwiseScissor, 0, 0, (int)drawData.DisplaySize.X, (int)drawData.DisplaySize.Y, this.Shader.Ps, this.Device.BlendStates.NonPreMultiplied, this.Device.DepthStencilStates.None);
        context.OM.SetRenderTargetToBackBuffer();

        context.IA.SetVertexBuffer(this.VertexBuffer);
        context.IA.SetIndexBuffer(this.IndexBuffer);
        context.VS.SetConstantBuffer(UserInterface.ConstantsSlot, this.User.ConstantsBuffer);
        context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);
    }

    private static ITexture2D CreateFontsTexture(Device device)
    {
        var io = ImGui.GetIO();
        unsafe
        {
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out var width, out var height);
            var format = Format.R8G8B8A8_UNorm; // Texture contains only white pixels for the font so gamma is irrelevant
            var pixelSpan = new Span<byte>(pixels, width * height * format.SizeOfInBytes());

            var texture = new Texture2D(device, width, height, format, false, nameof(ImGuiRenderer), "Font");
            texture.SetPixels(device, pixelSpan);
            return texture;
        }
    }
}
