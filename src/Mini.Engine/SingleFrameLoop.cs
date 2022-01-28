using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Debugging;
using Mini.Engine.Graphics.Textures.Generators;
using Mini.Engine.Windows;

namespace Mini.Engine;

[Service]
internal sealed class SingleFrameLoop : IGameLoop
{
    private int drawCalls;
    private readonly Win32Window Window;
    private readonly IrradianceGenerator Generator;
    private readonly ContentManager Content;

    private readonly RenderDoc? RenderDoc;

    public SingleFrameLoop(Win32Window window, IrradianceGenerator generator, ContentManager content, Services services)
    {
        this.Window = window;
        this.Generator = generator;
        this.Content = content;

        if (services.TryResolve<RenderDoc>(out var instance))
        {
            this.RenderDoc = instance;
        }
    }

    private void DrawExperiment()
    {
        var texture = this.Content.LoadTexture(@"Skyboxes\industrial.hdr");
        using var cube = this.Generator.Generate(texture, "CubeMap");
    }

    public void Update(float time, float elapsed) { }

    public void Draw(float alpha)
    {
        if(this.drawCalls == 0 && this.RenderDoc != null)
        {
            this.RenderDoc.TriggerCapture();
        }

        if (this.drawCalls == 1)
        {
            this.DrawExperiment();
            this.Window.Dispose(); // TODO: find nicer way to quit            
        }

        this.drawCalls++;
    }
    
    public void Dispose()
    {
        if (this.RenderDoc != null)
        {
            var path = this.RenderDoc.GetCapture(this.RenderDoc.GetNumCaptures() - 1) ?? string.Empty;
            this.RenderDoc.LaunchReplayUI(path);            
        }
    }
}
