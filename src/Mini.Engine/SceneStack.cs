using Mini.Engine.Configuration;

namespace Mini.Engine;

[Service]
public sealed class SceneStack
{
    private readonly LinkedList<IGameLoop> Scenes;

    public SceneStack()
    {
        this.Scenes = new LinkedList<IGameLoop>();
    }

    public void Push(IGameLoop scene)
    {
        this.Scenes.AddFirst(scene);
        scene.Enter();
    }

    public IGameLoop Peek()
    {
        return this.GetFirstOrThrow();
    }

    public void ReplaceTop(IGameLoop scene)
    {
        this.Pop();
        this.Push(scene);
    }

    public void Pop()
    {
        var first = this.GetFirstOrThrow();
        this.Scenes.RemoveFirst();

        first.Exit();
    }

    public void Clear()
    {
        while (this.Scenes.Count > 0)
        {
            this.Pop();
        }
    }

    // Iterate while allowing modifications to be made
    public void ForEach(Action<IGameLoop> action)
    {
        var node = this.Scenes.First;
        while (node != null)
        {
            action(node.Value);
            node = node.Next;
        }
    }

    private IGameLoop GetFirstOrThrow()
    {
        if (this.Scenes.First == null)
        {
            throw new InvalidOperationException("Stack is empty");
        }

        return this.Scenes.First.Value;
    }
}