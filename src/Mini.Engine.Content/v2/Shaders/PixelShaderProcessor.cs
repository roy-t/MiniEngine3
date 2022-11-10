using Mini.Engine.Content.v2.Serialization;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Resources.Shaders;

namespace Mini.Engine.Content.v2.Shaders;
internal sealed class PixelShaderProcessor : ShaderProcessor<IPixelShader, PixelShaderContent, PixelShaderSettings>
{
    private static readonly Guid HeaderPixelShader = new ("{3A6DC5B3-9859-463F-860F-A2FA39EBEDCB}");
    private const int VersionPixelShader = 1;

    public PixelShaderProcessor(Device device)
        : base(device, HeaderPixelShader, VersionPixelShader) { }

    public override string Profile => "ps_5_0";

    protected override void WriteSettings(ContentId id, PixelShaderSettings settings, ContentWriter writer)
    {
        // Do nothing
    }

    protected override IPixelShader Load(ContentId contentId, PixelShaderSettings settings, byte[] byteCode)
    {
        var name = DebugNameGenerator.GetName(contentId.ToString(), "PIXELSHADER");
        return new PixelShader(this.Device, name, byteCode);
    }

    protected override PixelShaderSettings ReadSettings(ContentId id, ContentReader reader)
    {
        return PixelShaderSettings.Empty;
    }

    public override PixelShaderContent Wrap(ContentId id, IPixelShader content, PixelShaderSettings settings, ISet<string> dependencies)
    {
        return new PixelShaderContent(id, content, settings, dependencies);
    }
}
