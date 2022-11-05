using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;

namespace Mini.Engine.Content.v2.Shaders;
internal sealed class ComputeShaderProcessor : ShaderProcessor<IComputeShader, ComputeShaderContent, ComputeShaderSettings>
{
    private static readonly Guid HeaderComputeShader = new("{8891B57B-C52C-4933-B121-FE6C718DB3D7}");
    private const int VersionComputeShader = 1;

    public ComputeShaderProcessor(Device device)
        : base(device, HeaderComputeShader, VersionComputeShader) { }

    public override string Profile => "cs_5_0";

    protected override void WriteSettings(ContentWriter writer, ComputeShaderSettings settings)
    {
        writer.Writer.Write(settings.NumThreadsX);
        writer.Writer.Write(settings.NumThreadsY);
        writer.Writer.Write(settings.NumThreadsZ);
    }

    protected override IComputeShader Load(ContentId contentId, ComputeShaderSettings settings, byte[] byteCode)
    {
        return new ComputeShader(this.Device, byteCode, settings.NumThreadsX, settings.NumThreadsY, settings.NumThreadsZ);
    }

    protected override ComputeShaderSettings LoadSetings(ContentReader reader)
    {
        var numThreadsX = reader.Reader.ReadInt32();
        var numThreadsY = reader.Reader.ReadInt32();
        var numThreadsZ = reader.Reader.ReadInt32();

        return new ComputeShaderSettings(numThreadsX, numThreadsY, numThreadsZ);
    }

    public override ComputeShaderContent Wrap(ContentId id, IComputeShader content, ComputeShaderSettings settings, ISet<string> dependencies)
    {
        return new ComputeShaderContent(id, content, settings, dependencies);
    }
}
