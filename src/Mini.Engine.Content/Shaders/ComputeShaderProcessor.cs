using Mini.Engine.Content.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;

namespace Mini.Engine.Content.Shaders;
internal sealed class ComputeShaderProcessor : ShaderProcessor<IComputeShader, ComputeShaderContent, ComputeShaderSettings>
{
    private static readonly Guid HeaderComputeShader = new("{8891B57B-C52C-4933-B121-FE6C718DB3D7}");
    private const int VersionComputeShader = 1;

    public ComputeShaderProcessor(Device device)
        : base(device, HeaderComputeShader, VersionComputeShader) { }

    public override string Profile => "cs_5_0";

    protected override void WriteSettings(ContentId id, ComputeShaderSettings settings, ContentWriter writer)
    {
        writer.Write(settings);
    }

    protected override IComputeShader Load(ContentId contentId, ComputeShaderSettings settings, byte[] byteCode)
    {
        var name = DebugNameGenerator.GetName(contentId.ToString(), "COMPUTESHADER");
        return new ComputeShader(this.Device, name, byteCode, settings.NumThreadsX, settings.NumThreadsY, settings.NumThreadsZ);
    }

    protected override ComputeShaderSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return reader.ReadComputeShaderSettings();
    }

    public override ComputeShaderContent Wrap(ContentId id, IComputeShader content, ComputeShaderSettings settings, ISet<string> dependencies)
    {
        return new ComputeShaderContent(id, content, settings, dependencies);
    }
}
