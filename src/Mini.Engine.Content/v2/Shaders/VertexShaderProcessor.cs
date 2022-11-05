using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;

namespace Mini.Engine.Content.v2.Shaders;
internal sealed class VertexShaderProcessor : ShaderProcessor<IVertexShader, VertexShaderContent, VertexShaderSettings>
{
    private static readonly Guid HeaderVertexShader = new("{7F3E4880-E395-473F-8A9F-7F6B78C624F6}");
    private const int VersionVertexShader = 1;

    public VertexShaderProcessor(Device device)
        : base(device, HeaderVertexShader, VersionVertexShader) { }

    public override string Profile => "vs_5_0";

    protected override void WriteSettings(ContentWriter writer, VertexShaderSettings settings)
    {
        // Do nothing
    }

    protected override IVertexShader Load(ContentId contentId, VertexShaderSettings settings, byte[] byteCode)
    {
        return new VertexShader(this.Device, byteCode);
    }

    protected override VertexShaderSettings LoadSetings(ContentReader reader)
    {
        return VertexShaderSettings.Empty;
    }

    public override VertexShaderContent Wrap(ContentId id, IVertexShader content, VertexShaderSettings settings, ISet<string> dependencies)
    {
        return new VertexShaderContent(id, content, settings, dependencies);
    }
}
