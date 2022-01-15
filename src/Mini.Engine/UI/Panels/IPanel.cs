namespace Mini.Engine.UI.Panels;

public interface IPanel
{
    public string Title { get; }
    public void Update(float elapsed);
}
