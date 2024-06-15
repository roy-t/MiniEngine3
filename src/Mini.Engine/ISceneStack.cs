namespace Mini.Engine;

public interface ISceneStack
{
    void Pop();
    void Push(IGameLoop scene);
    void ReplaceTop(IGameLoop scene);
}