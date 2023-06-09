using Mini.Engine.DirectX;
using Mini.Engine.Graphics.Lighting;
using Mini.Engine.Graphics.Models;
using Mini.Engine.Graphics.PostProcessing;

namespace Mini.Engine.Graphics;

// TODO: using something like this would be nice instad of passing x,y,w,h every time, and FrameService does too much
public sealed class RenderOutput
{
    public RenderOutput(Device device)
    {
        this.GBuffer = new GeometryBuffer(device);
        this.LBuffer = new LightBuffer(device);
        this.PBuffer = new PostProcessingBuffer(device);
    }

    public GeometryBuffer GBuffer { get; }
    public LightBuffer LBuffer { get; }
    public PostProcessingBuffer PBuffer { get; }

    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Heigth { get; set; }
}

