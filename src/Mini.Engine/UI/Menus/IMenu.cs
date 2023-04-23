namespace Mini.Engine.UI.Menus;

public interface IMenu
{
    public string Title { get; }
    public void Update(float elapsed);
}

public interface IEditorMenu : IMenu
{
}


public interface IDieselMenu : IMenu
{ }
