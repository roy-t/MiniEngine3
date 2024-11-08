﻿using Mini.Engine.Configuration;
using Mini.Engine.Debugging;
using Mini.Engine.Windows;

namespace Mini.Engine;

[Service]
internal sealed class SingleFrameLoop : IGameLoop
{
    private int drawCalls;
    private readonly Win32Window Window;
    private readonly RenderDoc? RenderDoc;

    public SingleFrameLoop(Win32Window window, RenderDoc? renderDoc = null)
    {
        this.Window = window;
        this.RenderDoc = renderDoc;
    }

    private void DrawExperiment()
    {
        // ...
    }

    public void HandleInput(float elapsedRealWorldTime)
    {

    }

    public void Simulate() { }

    public void Frame(float alpha, float elapsedRealWorldTime)
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

    public void Dispose()
    {
        if (this.RenderDoc != null)
        {
            var path = this.RenderDoc.GetCapture(this.RenderDoc.GetNumCaptures() - 1) ?? string.Empty;
            this.RenderDoc.LaunchReplayUI(path);
        }
    }
}
