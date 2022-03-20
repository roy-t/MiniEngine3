namespace Mini.Engine.UI.Menus;

public interface IMenu
{
    public string Title { get; }
    public void Update(float elapsed);
}
