using System;
using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.Graphics.World;
using Mini.Engine.Windows;

namespace Mini.Engine;

[Service]
internal sealed class SingleFrameLoop : IGameLoop
{
    private int drawCalls;
    private readonly Win32Window Window;
    private readonly SimplexNoiseGenerator NoiseGenerator;
    private readonly RenderDoc? RenderDoc;

    public SingleFrameLoop(Win32Window window, SimplexNoiseGenerator noiseGenerator, Services services)
    {
        this.Window = window;
        this.NoiseGenerator = noiseGenerator;
        if (services.TryResolve<RenderDoc>(out var instance))
        {
            this.RenderDoc = instance;
        }
    }

    private void DrawExperiment()
    {
        var dimensions = 64;
        var heightMap = this.NoiseGenerator.Generate(dimensions);
        var model = HeightMapTriangulator.Triangulate(heightMap, dimensions);
    }

    public void Update(float time, float elapsed) { }

    public void Draw(float alpha)
    {
        if (this.drawCalls == 0 && this.RenderDoc != null)
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

    public void Resize(int width, int height)
    {
        throw new NotSupportedException($"{nameof(SingleFrameLoop)} should not be resized during its short lifetime");
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
