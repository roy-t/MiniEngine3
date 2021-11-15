namespace Mini.Engine.Windows.Events;

public readonly struct SizeEventArgs
{
    public SizeEventArgs(int width, int height)
    {
        this.Width = width;
        this.Height = height;
    }

    public int Width { get; }
    public int Height { get; }
}
