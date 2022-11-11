using Mini.Engine.Content.Serialization;
using Mini.Engine.Content.Textures;

namespace Mini.Engine.Content.Materials;
public static class SerializationExtensions
{
    public static void Write(this ContentWriter writer, MaterialSettings materialSettings)
    {
        writer.Write(materialSettings.AlbedoFormat);
        writer.Write(materialSettings.MetalicnessFormat);
        writer.Write(materialSettings.NormalFormat);
        writer.Write(materialSettings.RoughnessFormat);
        writer.Write(materialSettings.AmbientOcclusionFormat);
    }

    public static MaterialSettings ReadMaterialSettings(this ContentReader reader)
    {
        var albedo = reader.ReadTextureSettings();
        var metalicness = reader.ReadTextureSettings();
        var normal = reader.ReadTextureSettings();
        var roughness = reader.ReadTextureSettings();
        var ambientOcclusion = reader.ReadTextureSettings();

        return new MaterialSettings(albedo, metalicness, normal, roughness, ambientOcclusion);
    }
}
