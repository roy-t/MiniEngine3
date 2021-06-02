﻿//based on https://github.com/ocornut/imgui/blob/master/examples/imgui_impl_dx11.cpp

using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Mini.Engine.DirectX;
using Vortice;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Vortice.Mathematics;
using ImDrawIdx = System.UInt16;

namespace VorticeImGui
{
    unsafe public class ImGuiRenderer
    {
        ID3D11Device device;
        ID3D11DeviceContext deviceContext;

        ID3D11InputLayout inputLayout;
        ID3D11SamplerState fontSampler;
        ID3D11RasterizerState rasterizerState;
        ID3D11BlendState blendState;
        ID3D11DepthStencilState depthStencilState;

        private readonly Shader Shader;
        private readonly VertexBuffer<ImDrawVert> VertexBuffer;
        private readonly IndexBuffer<ImDrawIdx> IndexBuffer;
        private readonly ConstantBuffer<Matrix4x4> ConstantBuffer;
        private Texture2D fontTexture;

        Dictionary<IntPtr, ID3D11ShaderResourceView> textureResources = new Dictionary<IntPtr, ID3D11ShaderResourceView>();

        public ImGuiRenderer(ID3D11Device device, ID3D11DeviceContext deviceContext)
        {
            this.device = device;
            this.deviceContext = deviceContext;

            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

            this.VertexBuffer = new VertexBuffer<ImDrawVert>(device, deviceContext);
            this.IndexBuffer = new IndexBuffer<ImDrawIdx>(device, deviceContext);
            this.ConstantBuffer = new ConstantBuffer<Matrix4x4>(device, deviceContext);

            this.Shader = new Shader(device, "../../../../Mini.Engine.Content/Shaders/Immediate.hlsl");
            inputLayout = this.Shader.CreateInputLayout
            (
                new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 8, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0, InputClassification.PerVertexData, 0)
            );

            CreateDeviceObjects();
        }

        public void Render(ImDrawDataPtr data)
        {
            if (data.DisplaySize.X <= 0.0f || data.DisplaySize.Y <= 0.0f || data.TotalVtxCount <= 0)
            {
                return;
            }

            ID3D11DeviceContext ctx = deviceContext;

            this.VertexBuffer.EnsureCapacity(data.TotalVtxCount, data.TotalVtxCount / 10);
            this.IndexBuffer.EnsureCapacity(data.TotalIdxCount, data.TotalIdxCount / 10);

            var vertexOffset = 0;
            var indexOffset = 0;
            using (var vertexWriter = this.VertexBuffer.OpenWriter())
            using (var indexWriter = this.IndexBuffer.OpenWriter())
            {
                for (var n = 0; n < data.CmdListsCount; n++)
                {
                    var cmdlList = data.CmdListsRange[n];

                    vertexWriter.MapData(new Span<ImDrawVert>(cmdlList.VtxBuffer.Data.ToPointer(), cmdlList.VtxBuffer.Size), vertexOffset);
                    vertexOffset += cmdlList.VtxBuffer.Size;

                    indexWriter.MapData(new Span<ImDrawIdx>(cmdlList.IdxBuffer.Data.ToPointer(), cmdlList.IdxBuffer.Size), indexOffset);
                    indexOffset += cmdlList.IdxBuffer.Size;
                }
            }

            // Setup orthographic projection matrix into our constant buffer
            var mat = Matrix4x4.CreateOrthographicOffCenter(0, data.DisplaySize.X, data.DisplaySize.Y, 0, -1.0f, 1.0f);
            this.ConstantBuffer.MapData(mat);

            SetupRenderState(data, ctx);

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
                        var rect = new RawRect((int)(cmd.ClipRect.X - clip_off.X), (int)(cmd.ClipRect.Y - clip_off.Y), (int)(cmd.ClipRect.Z - clip_off.X), (int)(cmd.ClipRect.W - clip_off.Y));
                        ctx.RSSetScissorRects(new[] { rect });

                        textureResources.TryGetValue(cmd.TextureId, out var texture);
                        if (texture != null)
                            ctx.PSSetShaderResources(0, new[] { texture });

                        ctx.DrawIndexed((int)cmd.ElemCount, (int)(cmd.IdxOffset + global_idx_offset), (int)(cmd.VtxOffset + global_vtx_offset));
                    }
                }
                global_idx_offset += cmdList.IdxBuffer.Size;
                global_vtx_offset += cmdList.VtxBuffer.Size;
            }
        }

        public void Dispose()
        {
            if (device == null)
                return;

            InvalidateDeviceObjects();

            ReleaseAndNullify(ref device);
            ReleaseAndNullify(ref deviceContext);
        }

        void ReleaseAndNullify<T>(ref T o) where T : SharpGen.Runtime.ComObject
        {
            o.Release();
            o = null;
        }

        void SetupRenderState(ImDrawDataPtr drawData, ID3D11DeviceContext ctx)
        {
            var viewport = new Viewport(
                0,
                0,
                drawData.DisplaySize.X,
                drawData.DisplaySize.Y,
                0.0f,
                1.0f
            );
            ctx.RSSetViewports(new[] { viewport });

            this.Shader.Set(ctx);

            int stride = sizeof(ImDrawVert);
            int offset = 0;
            ctx.IASetInputLayout(inputLayout);
            ctx.IASetVertexBuffers(0, 1, new[] { VertexBuffer.Buffer }, new[] { stride }, new[] { offset });
            ctx.IASetIndexBuffer(IndexBuffer.Buffer, sizeof(ImDrawIdx) == 2 ? Format.R16_UInt : Format.R32_UInt, 0);
            ctx.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            ctx.VSSetConstantBuffers(0, new[] { ConstantBuffer.Buffer });
            ctx.PSSetSamplers(0, new[] { fontSampler });
            ctx.GSSetShader(null);
            ctx.HSSetShader(null);
            ctx.DSSetShader(null);
            ctx.CSSetShader(null);

            ctx.OMSetBlendState(blendState);
            ctx.OMSetDepthStencilState(depthStencilState);
            ctx.RSSetState(rasterizerState);
        }

        void CreateFontsTexture()
        {
            var io = ImGui.GetIO();
            byte* pixels;
            int width, height;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width, out height);

            var format = Format.R8G8B8A8_UNorm;
            var pixelSpan = new Span<byte>(pixels, width * height * format.SizeOfInBytes());
            this.fontTexture = new Texture2D(device, deviceContext, pixelSpan, width, height, format, false, "ImGui_Font");
            io.Fonts.TexID = RegisterTexture(fontTexture.ShaderResourceView);

            var samplerDesc = new SamplerDescription
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Wrap,
                AddressV = TextureAddressMode.Wrap,
                AddressW = TextureAddressMode.Wrap,
                MipLODBias = 0f,
                ComparisonFunction = ComparisonFunction.Always,
                MinLOD = 0f,
                MaxLOD = 0f
            };
            fontSampler = device.CreateSamplerState(samplerDesc);
        }

        IntPtr RegisterTexture(ID3D11ShaderResourceView texture)
        {
            var imguiID = texture.NativePointer;
            textureResources.Add(imguiID, texture);

            return imguiID;
        }

        void CreateDeviceObjects()
        {
            var blendDesc = new BlendDescription
            {
                AlphaToCoverageEnable = false
            };

            blendDesc.RenderTarget[0] = new RenderTargetBlendDescription
            {
                IsBlendEnabled = true,
                SourceBlend = Blend.SourceAlpha,
                DestinationBlend = Blend.InverseSourceAlpha,
                BlendOperation = BlendOperation.Add,
                SourceBlendAlpha = Blend.InverseSourceAlpha,
                DestinationBlendAlpha = Blend.Zero,
                BlendOperationAlpha = BlendOperation.Add,
                RenderTargetWriteMask = ColorWriteEnable.All
            };

            blendState = device.CreateBlendState(blendDesc);

            var rasterDesc = new RasterizerDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                ScissorEnable = true,
                DepthClipEnable = true
            };

            rasterizerState = device.CreateRasterizerState(rasterDesc);

            var stencilOpDesc = new DepthStencilOperationDescription(StencilOperation.Keep, StencilOperation.Keep, StencilOperation.Keep, ComparisonFunction.Always);
            var depthDesc = new DepthStencilDescription
            {
                DepthEnable = false,
                DepthWriteMask = DepthWriteMask.All,
                DepthFunc = ComparisonFunction.Always,
                StencilEnable = false,
                FrontFace = stencilOpDesc,
                BackFace = stencilOpDesc
            };

            depthStencilState = device.CreateDepthStencilState(depthDesc);

            CreateFontsTexture();
        }

        void InvalidateDeviceObjects()
        {
            ReleaseAndNullify(ref fontSampler);
            //ReleaseAndNullify(ref fontTextureView);
            //ReleaseAndNullify(ref indexBuffer);
            //ReleaseAndNullify(ref vertexBuffer);
            ReleaseAndNullify(ref blendState);
            ReleaseAndNullify(ref depthStencilState);
            ReleaseAndNullify(ref rasterizerState);
            //ReleaseAndNullify(ref constantBuffer);
            ReleaseAndNullify(ref inputLayout);

            this.Shader?.Dispose();
        }
    }
}