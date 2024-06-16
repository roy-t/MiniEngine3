using System.Collections;
using Mini.Engine.Configuration;

namespace Mini.Engine;

[Service]
public sealed class SceneStack : IEnumerable<IGameLoop>
{
    private readonly Stack<IGameLoop> Scenes;

    public SceneStack()
    {
        this.Scenes = new Stack<IGameLoop>(10);
    }

    public void Push(IGameLoop scene)
    {
        this.Scenes.Push(scene);
    }

    public IGameLoop Peek()
    {
        return this.Scenes.Peek();
    }

    public void ReplaceTop(IGameLoop scene)
    {
        this.Scenes.Pop();
        this.Scenes.Push(scene);
    }

    public void Pop()
    {
        this.Scenes.Pop();
    }

    public IEnumerator<IGameLoop> GetEnumerator()
    {
        return this.Scenes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.Scenes.GetEnumerator();
    }
}