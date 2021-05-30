//based on https://github.com/ocornut/imgui/blob/master/examples/imgui_impl_dx11.cpp

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
        const int VertexConstantBufferSize = 16 * 4;

        ID3D11Device device;
        ID3D11DeviceContext deviceContext;
        //ID3D11Buffer vertexBuffer;
        VertexBuffer vertexBuffer;
        ID3D11Buffer indexBuffer;
        ID3D11InputLayout inputLayout;
        ID3D11Buffer constantBuffer;
        ID3D11SamplerState fontSampler;
        ID3D11ShaderResourceView fontTextureView;
        ID3D11RasterizerState rasterizerState;
        ID3D11BlendState blendState;
        ID3D11DepthStencilState depthStencilState;
        int vertexBufferSize = 5000, indexBufferSize = 10000;

        Shader shader;

        Dictionary<IntPtr, ID3D11ShaderResourceView> textureResources = new Dictionary<IntPtr, ID3D11ShaderResourceView>();

        public ImGuiRenderer(ID3D11Device device, ID3D11DeviceContext deviceContext)
        {
            this.device = device;
            this.deviceContext = deviceContext;

            device.AddRef();
            deviceContext.AddRef();

            var io = ImGui.GetIO();
            io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;  // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.

            this.vertexBuffer = new VertexBuffer(this.device, this.deviceContext);

            CreateDeviceObjects();
        }

        public void Render(ImDrawDataPtr data)
        {
            // Avoid rendering when minimized
            if (data.DisplaySize.X <= 0.0f || data.DisplaySize.Y <= 0.0f || data.TotalVtxCount <= 0.0f)
                return;

            ID3D11DeviceContext ctx = deviceContext;


            this.vertexBuffer.Reserve(data.TotalVtxCount, sizeof(ImDrawVert));
            //if (vertexBuffer == null || vertexBufferSize < data.TotalVtxCount)
            //{
            //    vertexBuffer?.Release();

            //    vertexBufferSize = data.TotalVtxCount + 5000;
            //    BufferDescription desc = new BufferDescription();
            //    desc.Usage = Vortice.Direct3D11.Usage.Dynamic;
            //    desc.SizeInBytes = vertexBufferSize * sizeof(ImDrawVert);
            //    desc.BindFlags = BindFlags.VertexBuffer;
            //    desc.CpuAccessFlags = CpuAccessFlags.Write;
            //    vertexBuffer = device.CreateBuffer(desc);
            //}

            if (indexBuffer == null || indexBufferSize < data.TotalIdxCount)
            {
                indexBuffer?.Release();

                indexBufferSize = data.TotalIdxCount + 10000;

                BufferDescription desc = new BufferDescription();
                desc.Usage = Vortice.Direct3D11.Usage.Dynamic;
                desc.SizeInBytes = indexBufferSize * sizeof(ImDrawIdx);
                desc.BindFlags = BindFlags.IndexBuffer;
                desc.CpuAccessFlags = CpuAccessFlags.Write;
                indexBuffer = device.CreateBuffer(desc);
            }

            // Upload vertex/index data into a single contiguous GPU buffer
            //var vertexResource = ctx.Map(vertexBuffer, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
            var indexResource = ctx.Map(indexBuffer, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
            //var vertexResourcePointer = (ImDrawVert*)vertexResource.DataPointer;
            int vertexBufferOffset = 0;
            var indexResourcePointer = (ImDrawIdx*)indexResource.DataPointer;
            for (int n = 0; n < data.CmdListsCount; n++)
            {
                var cmdlList = data.CmdListsRange[n];
                // TODO figure out why adding a main menu bar and then opening it causes a triangle flash!
                var vertBytes = cmdlList.VtxBuffer.Size * sizeof(ImDrawVert);
                //Buffer.MemoryCopy((void*)cmdlList.VtxBuffer.Data, vertexResourcePointer, vertBytes, vertBytes);
                this.vertexBuffer.MapData(cmdlList.VtxBuffer.Data, cmdlList.VtxBuffer.Size, sizeof(ImDrawVert), vertexBufferOffset);

                var idxBytes = cmdlList.IdxBuffer.Size * sizeof(ImDrawIdx);
                Buffer.MemoryCopy((void*)cmdlList.IdxBuffer.Data, indexResourcePointer, idxBytes, idxBytes);

                //vertexResourcePointer += cmdlList.VtxBuffer.Size;
                vertexBufferOffset += cmdlList.VtxBuffer.Size;
                indexResourcePointer += cmdlList.IdxBuffer.Size;
            }
            //ctx.Unmap(vertexBuffer, 0);
            ctx.Unmap(indexBuffer, 0);




            // Setup orthographic projection matrix into our constant buffer
            // Our visible imgui space lies from draw_data.DisplayPos (top left) to draw_data.DisplayPos+data_data.DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.

            var constResource = ctx.Map(constantBuffer, 0, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
            var span = constResource.AsSpan<float>(VertexConstantBufferSize);
            float L = data.DisplayPos.X;
            float R = data.DisplayPos.X + data.DisplaySize.X;
            float T = data.DisplayPos.Y;
            float B = data.DisplayPos.Y + data.DisplaySize.Y;
            float[] mvp =
            {
                    2.0f/(R-L),   0.0f,           0.0f,       0.0f,
                    0.0f,         2.0f/(T-B),     0.0f,       0.0f,
                    0.0f,         0.0f,           0.5f,       0.0f,
                    (R+L)/(L-R),  (T+B)/(B-T),    0.5f,       1.0f,
            };
            mvp.CopyTo(span);
            ctx.Unmap(constantBuffer, 0);

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

            this.shader.Set(ctx);

            int stride = sizeof(ImDrawVert);
            int offset = 0;
            ctx.IASetInputLayout(inputLayout);
            ctx.IASetVertexBuffers(0, 1, new[] { vertexBuffer.Buffer }, new[] { stride }, new[] { offset });
            ctx.IASetIndexBuffer(indexBuffer, sizeof(ImDrawIdx) == 2 ? Format.R16_UInt : Format.R32_UInt, 0);
            ctx.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            ctx.VSSetConstantBuffers(0, new[] { constantBuffer });
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

            var texDesc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                MipLevels = 1,
                ArraySize = 1,
                Format = Format.R8G8B8A8_UNorm,
                SampleDescription = new SampleDescription { Count = 1 },
                Usage = Vortice.Direct3D11.Usage.Default,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None
            };

            var subResource = new SubresourceData
            {
                DataPointer = (IntPtr)pixels,
                Pitch = texDesc.Width * 4,
                SlicePitch = 0
            };

            var texture = device.CreateTexture2D(texDesc, new[] { subResource });

            var resViewDesc = new ShaderResourceViewDescription
            {
                Format = Format.R8G8B8A8_UNorm,
                ViewDimension = ShaderResourceViewDimension.Texture2D,
                Texture2D = new Texture2DShaderResourceView { MipLevels = texDesc.MipLevels, MostDetailedMip = 0 }
            };
            fontTextureView = device.CreateShaderResourceView(texture, resViewDesc);
            texture.Release();

            io.Fonts.TexID = RegisterTexture(fontTextureView);

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
            this.shader = new Shader(device, "../../../../Mini.Engine.Content/Shaders/Immediate.hlsl");
            inputLayout = this.shader.CreateInputLayout
            (
                new InputElementDescription("POSITION", 0, Format.R32G32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("TEXCOORD", 0, Format.R32G32_Float, 8, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("COLOR", 0, Format.R8G8B8A8_UNorm, 16, 0, InputClassification.PerVertexData, 0)
            );

            var constBufferDesc = new BufferDescription
            {
                SizeInBytes = VertexConstantBufferSize,
                Usage = Vortice.Direct3D11.Usage.Dynamic,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.Write
            };
            constantBuffer = device.CreateBuffer(constBufferDesc);

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
            ReleaseAndNullify(ref fontTextureView);
            ReleaseAndNullify(ref indexBuffer);
            ReleaseAndNullify(ref blendState);
            ReleaseAndNullify(ref depthStencilState);
            ReleaseAndNullify(ref rasterizerState);
            ReleaseAndNullify(ref constantBuffer);
            ReleaseAndNullify(ref inputLayout);

            this.shader?.Dispose();
            this.vertexBuffer?.Dispose();
        }
    }
}