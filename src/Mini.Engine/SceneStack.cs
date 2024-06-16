using System.Collections;
using Mini.Engine.Configuration;

namespace Mini.Engine;

[Service]
public sealed class SceneStack : IEnumerable<IGameLoop>
{
    private readonly LinkedList<IGameLoop> Scenes;

    public SceneStack()
    {
        this.Scenes = new LinkedList<IGameLoop>();
    }

    public void Push(IGameLoop scene)
    {
        this.Scenes.AddFirst(scene);
    }

    public IGameLoop Peek()
    {
        if (this.Scenes.First == null)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        return this.Scenes.First.Value;
    }

    public void ReplaceTop(IGameLoop scene)
    {
        if (this.Scenes.First == null)
        {
            throw new InvalidOperationException("Stack is empty");
        }
        this.Scenes.First.Value = scene;
    }

    public void Pop()
    {
        if (this.Scenes.First == null)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        this.Scenes.RemoveFirst();
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