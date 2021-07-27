//based on https://github.com/ocornut/imgui/blob/master/examples/imgui_impl_dx11.cpp

using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.DirectX;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using ImDrawIdx = System.UInt16;

namespace VorticeImGui
{
    public class ImGuiRenderer
    {
        private int textureCounter;

        // TODO: how to abstract this/ the InputElementDescription, there's nothing to add to it with a separate type
        // tbh and it would also require a lot of duplication of enums etc..        
        private readonly Dictionary<IntPtr, Texture2D> TextureResources;

        // Borrowed resources
        private readonly Device Device;
        private readonly ImmediateDeviceContext ImmediateContext;

        // Created resources        
        private readonly DeferredDeviceContext DeferredContext;
        private readonly Shader Shader;
        private readonly Texture2D FontTexture;
        private readonly InputLayout InputLayout;
        private readonly VertexBuffer<ImDrawVert> VertexBuffer;
        private readonly IndexBuffer<ImDrawIdx> IndexBuffer;
        private readonly ConstantBuffer<Matrix4x4> ConstantBuffer;

        public ImGuiRenderer(Device device)
        {
            this.Device = device;
            this.ImmediateContext = device.ImmediateContext;
            this.DeferredContext = device.CreateDeferredContext();

            this.VertexBuffer = new VertexBuffer<ImDrawVert>(device);
            this.IndexBuffer = new IndexBuffer<ImDrawIdx>(device);
            this.ConstantBuffer = new ConstantBuffer<Matrix4x4>(device);

            this.Shader = new Shader(device, "../../../../Mini.Engine.Content/Shaders/Immediate.fx");
            this.InputLayout = this.Shader.CreateInputLayout
            (
                new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 8, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0, InputClassification.PerVertexData, 0)
            );

            this.TextureResources = new Dictionary<IntPtr, Texture2D>();
            this.FontTexture = CreateFontsTexture(device);

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;
            io.Fonts.TexID = this.RegisterTexture(this.FontTexture);
        }

        public void Render(ImDrawDataPtr data, RenderTarget2D renderTarget)
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
            var mat = Matrix4x4.CreateOrthographicOffCenter(0, data.DisplaySize.X, data.DisplaySize.Y, 0, -1.0f, 1.0f);
            this.ConstantBuffer.MapData(this.DeferredContext, mat);

            this.SetupRenderState(data, this.DeferredContext, renderTarget);

            // Render command lists
            // (Because we merged all buffers into a single one, we maintain our own offset into them)
            int global_idx_offset = 0;
            int global_vtx_offset = 0;
            Vector2 clip_off = data.DisplayPos;
            for (int n = 0; n < data.CmdListsCount; n++)
            {
                var cmdList = data.CmdListsRange[n];
                for (int i = 0; i < cmdList.CmdBuffer.Size; i++)
                {
                    var cmd = cmdList.CmdBuffer[i];
                    if (cmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException("user callbacks not implemented");
                    }
                    else
                    {
                        var left = (int)(cmd.ClipRect.X - clip_off.X);
                        var top = (int)(cmd.ClipRect.Y - clip_off.Y);
                        var right = (int)(cmd.ClipRect.Z - clip_off.X);
                        var bottom = (int)(cmd.ClipRect.W - clip_off.Y);

                        this.DeferredContext.RS.SetScissorRect(left, top, right - left, bottom - top);

                        if (this.TextureResources.TryGetValue(cmd.TextureId, out var texture))
                        {
                            this.DeferredContext.PS.SetShaderResource(0, texture);
                        }

                        this.DeferredContext.DrawIndexed((int)cmd.ElemCount, (int)(cmd.IdxOffset + global_idx_offset), (int)(cmd.VtxOffset + global_vtx_offset));
                    }
                }
                global_idx_offset += cmdList.IdxBuffer.Size;
                global_vtx_offset += cmdList.VtxBuffer.Size;
            }

            using var commandList = this.DeferredContext.FinishCommandList();
            this.ImmediateContext.ExecuteCommandList(commandList);
        }

        public void Dispose()
        {
            this.DeferredContext.Dispose();
            this.Shader.Dispose();
            this.ConstantBuffer.Dispose();
            this.IndexBuffer.Dispose();
            this.VertexBuffer.Dispose();
            this.FontTexture.Dispose();
            this.InputLayout.Dispose();
        }

        private void SetupRenderState(ImDrawDataPtr drawData, DeferredDeviceContext context, RenderTarget2D renderTarget)
        {
            context.OM.SetRenderTarget(renderTarget);
            context.RS.SetViewPort(0, 0, drawData.DisplaySize.X, drawData.DisplaySize.Y);

            this.Shader.Set(context);

            context.IA.SetInputLayout(this.InputLayout);

            context.IA.SetVertexBuffer(this.VertexBuffer);
            context.IA.SetIndexBuffer(this.IndexBuffer);

            context.IA.SetPrimitiveTopology(PrimitiveTopology.TriangleList);

            context.VS.SetConstantBuffer(0, this.ConstantBuffer);

            context.PS.SetSampler(0, this.Device.SamplerStates.LinearWrap);
            context.OM.SetBlendState(this.Device.BlendStates.AlphaBlend);
            context.OM.SetDepthStencilState(this.Device.DepthStencilStates.None);
            context.RS.SetState(this.Device.RasterizerStates.CullNone);
        }

        private IntPtr RegisterTexture(Texture2D texture)
        {
            var id = (IntPtr)textureCounter++;
            this.TextureResources.Add(id, texture);

            return id;
        }

        private static Texture2D CreateFontsTexture(Device device)
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
}