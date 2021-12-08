using System.IO;

namespace Mini.Engine.Content.Models;

internal static class TextureLookup
{
    public static void MakeTexturesPathsRelativeToContentPath(ModelData data)
    {
        var cwd = Path.GetDirectoryName(data.FileName) ?? string.Empty;

        for (var i = 0; i < data.Materials.Length; i++)
        {
            var material = data.Materials[i];
            data.Materials[i] = material with
            {
                Albedo = GetTexturePath(cwd, material.Albedo),
                Metalicness = GetTexturePath(cwd, material.Albedo),
                Normal = GetTexturePath(cwd, material.Albedo),
                Roughness = GetTexturePath(cwd, material.Albedo),
                AmbientOcclusion = GetTexturePath(cwd, material.Albedo),
            };
        }
    }

    public static void AddFallbackTextures(ModelData data)
    {
        for (var i = 0; i < data.Materials.Length; i++)
        {
            var material = data.Materials[i];
            data.Materials[i] = material with
            {
                Albedo = Fallback(material.Albedo, @"Models\albedo.tga"),
                Metalicness = Fallback(material.Albedo, @"Models\metalicness.tga"),
                Normal = Fallback(material.Albedo, @"Models\normal.tga"),
                Roughness = Fallback(material.Albedo, @"Models\roughness.tga"),
                AmbientOcclusion = Fallback(material.Albedo, @"Models\ao.tga"),
            };
        }
    }

    private static string Fallback(string? original, string fallback)
    {
        if (string.IsNullOrEmpty(original))
        {
            return fallback;
        }

        return original;
    }

    private static string GetTexturePath(string pathToModel, string pathToTexture)
    {
        if (string.IsNullOrEmpty(pathToTexture))
        {
            return string.Empty;
        }

        return Path.Combine(pathToModel, pathToTexture);
    }
}
