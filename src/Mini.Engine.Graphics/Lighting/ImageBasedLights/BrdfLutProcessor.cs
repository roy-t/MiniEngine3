﻿using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Serialization;
using Mini.Engine.Content.Shaders.Generated;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Surfaces;
using Mini.Engine.IO;
using StbImageSharp;
using Vortice.DXGI;
using ImageInfo = Mini.Engine.DirectX.Resources.Surfaces.ImageInfo;

namespace Mini.Engine.Graphics.Lighting.ImageBasedLights;

[Service]
public sealed class BrdfLutProcessor : ContentProcessor<ITexture, TextureContent, TextureSettings>, IDisposable
{
    private const int Resolution = 512;

    private const int ProcessorVersion = 1;
    private static readonly Guid ProcessorType = new("{0021262F-65A4-4D2C-AF2F-F6FEB7E62229}");

    private readonly Device Device;
    private readonly BrdfLutCompute Shader;
    private readonly BrdfLutCompute.User User;

    public BrdfLutProcessor(Device device, BrdfLutCompute shader)
        : base(device.Resources, ProcessorVersion, ProcessorType, ".hdr")
    {
        this.Device = device;      
        this.Shader = shader;

        this.User = this.Shader.CreateUserFor<BrdfLutProcessor>();
    }

    protected override IEnumerable<string> GetAdditionalDependencies()
    {
        return new[] { BrdfLutCompute.SourceFile };
    }

    protected override void WriteSettings(ContentId id, TextureSettings _, ContentWriter writer)
    {
        var settings = new TextureSettings(SuperCompressed.Mode.Linear, false, false);
        writer.Write(settings);
    }

    protected override void WriteBody(ContentId id, TextureSettings settings, ContentWriter writer, IReadOnlyVirtualFileSystem fileSystem)
    {
        var image = this.ComputeImage();                
        HdrTextureWriter.Write(writer, settings, image);
    }

    protected override TextureSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return reader.ReadTextureSettings();
    }

    protected override ITexture ReadBody(ContentId id, TextureSettings settings, ContentReader reader)
    {        
        return HdrTextureReader.Read(this.Device, id, reader, settings);
    }
   
    public override TextureContent Wrap(ContentId id, ITexture content, TextureSettings settings, ISet<string> dependencies)
    {
        return new TextureContent(id, content, settings, dependencies);
    }    

    private ImageResultFloat ComputeImage()
    {
        var context = this.Device.ImmediateContext;

        var imageInfo = new ImageInfo(Resolution, Resolution, Format.R32G32_Float);
        using var texture = new RWTexture(this.Device, "BrdfLut", imageInfo, MipMapInfo.None());
        this.User.MapConstants(context, Resolution, Resolution);
        context.CS.SetConstantBuffer(BrdfLutCompute.ConstantsSlot, this.User.ConstantsBuffer);
        context.CS.SetShader(this.Shader.BrdfLutKernel);
        context.CS.SetUnorderedAccessView(BrdfLutCompute.Lut, texture);

        var (x, y, z) = this.Shader.GetDispatchSizeForBrdfLutKernel(Resolution, Resolution, 1);
        context.CS.Dispatch(x, y, z);
        context.CS.ClearUnorderedAccessView(BrdfLutCompute.Lut);

        var buffer = new float[imageInfo.Pixels * 2];
        context.CopySurfaceDataToSpan<float>(texture, buffer);

        var image = new ImageResultFloat()
        {
            Width = Resolution,
            Height = Resolution,
            SourceComp = ColorComponents.GreyAlpha,
            Comp = ColorComponents.GreyAlpha,
            Data = buffer
        };

        return image;
    }

    public void Dispose()
    {
        this.User.Dispose();
    }
}
