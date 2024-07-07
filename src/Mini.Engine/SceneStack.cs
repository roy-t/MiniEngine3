using Mini.Engine.Configuration;

namespace Mini.Engine;

[Service]
public sealed class SceneStack
{
    private readonly LinkedList<IGameLoop> Scenes;
    private readonly LoadingGameLoop LoadingGameLoop;

    public SceneStack(LoadingGameLoop loadingGameLoop)
    {
        this.Scenes = new LinkedList<IGameLoop>();
        this.LoadingGameLoop = loadingGameLoop;
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
}