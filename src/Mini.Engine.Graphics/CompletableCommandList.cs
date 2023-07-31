using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.ECS.Systems;

namespace Mini.Engine.Graphics;
public sealed class CompletableCommandList : ICompletable
{ 
    private readonly ImmediateDeviceContext Context;
    private readonly CommandList CommandList;

    private CompletableCommandList(ImmediateDeviceContext context, CommandList commandList)
    {
        this.Context = context;
        this.CommandList = commandList;
    }

    public void Complete()
    {
        this.Context.ExecuteCommandList(this.CommandList);
        this.CommandList.Dispose();
    }

    public static ICompletable Create(ImmediateDeviceContext context, CommandList commandList)
    {
        return new CompletableCommandList(context, commandList);
    }
}
