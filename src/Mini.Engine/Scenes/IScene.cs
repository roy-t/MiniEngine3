using System.Collections.Generic;

namespace Mini.Engine.Scenes;

public interface IScene
{
    string Title { get; }
    IReadOnlyList<LoadAction> Load();    
}