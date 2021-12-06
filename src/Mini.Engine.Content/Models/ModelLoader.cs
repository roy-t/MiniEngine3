using System;
using System.IO;
using Mini.Engine.Content.Models.Wavefront;
using Mini.Engine.Content.Textures;
using Mini.Engine.DirectX;
using Mini.Engine.IO;

namespace Mini.Engine.Content.Models;

internal sealed class ModelLoader : IContentLoader<ModelContent>
{
    private readonly WavefrontModelDataLoader WaveFrontDataLoader;
    private readonly IContentLoader<Texture2DContent> TextureLoader;

    public ModelLoader(IVirtualFileSystem fileSystem, IContentLoader<Texture2DContent> textureLoader)
    {
        this.WaveFrontDataLoader = new WavefrontModelDataLoader(fileSystem);
        this.TextureLoader = textureLoader;
    }

    public ModelContent Load(Device device, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        IContentDataLoader<ModelData> loader = extension switch
        {
            ".obj" => this.WaveFrontDataLoader,
            _ => throw new NotSupportedException($"Could not load {fileName}. Unsupported model file type {extension}")
        };

        var cwd = Path.GetDirectoryName(fileName) ?? string.Empty;
        var data = loader.Load(fileName);

        for (var i = 0; i < data.Materials.Length; i++)
        {
            data.Materials[i] = FixTexturePathsAndSetFallbacks(data.Materials[i], cwd);
        }

        return new ModelContent(device, loader, this.TextureLoader, data, fileName);
    }

    private static MaterialData FixTexturePathsAndSetFallbacks(MaterialData reference, string searchDirectory)
    {
        var albedo = GetTexturePath(searchDirectory, reference.Albedo, @"Models\albedo.tga");
        var metalicness = GetTexturePath(searchDirectory, reference.Metalicness, @"Models\metalicness.tga");
        var normal = GetTexturePath(searchDirectory, reference.Normal, @"Models\normal.tga");
        var roughness = GetTexturePath(searchDirectory, reference.Roughness, @"Models\roughness.tga");
        var ambientOcclusion = GetTexturePath(searchDirectory, reference.AmbientOcclusion, @"Models\ao.tga");

        return new MaterialData(reference.FileName, reference.Index, albedo, metalicness, normal, roughness, ambientOcclusion);
    }

    private static string GetTexturePath(string searchDirectory, string name, string fallback)
    {
        if (!string.IsNullOrEmpty(name))
        {
            return Path.Combine(searchDirectory, name);
        }

        return fallback;
    }
}
