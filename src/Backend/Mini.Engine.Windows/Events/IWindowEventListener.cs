namespace Mini.Engine.Windows.Events;

public interface IWindowEventListener
{
    void OnSizeChanged(int width, int height);
    void OnFocusChanged(bool hasFocus);
    void OnDestroyed();
    void OnMouseEnter();
    void OnMouseMove();
    void OnMouseLeave();
}
