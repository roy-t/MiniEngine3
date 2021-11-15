namespace Mini.Engine.ECS.Components;

internal class ComponentStructTest
{
}


public interface ISEntity
{
    int Entity { get; }
}


public struct SComponentA : ISEntity
{
    public int Entity { get; }
}

public struct SComponentB : ISEntity
{
    public int Entity { get; }
}


public sealed class ServiceABinding
{
    private readonly ServiceA Service;
    private readonly SComponentA[] ContainerA;
    private readonly SComponentB[] ContainerB;

    public ServiceABinding()
    {
        this.Service = new ServiceA();
        this.ContainerA = new SComponentA[10];
        this.ContainerB = new SComponentB[10];
    }


    public void Process()
    {
        // TODO: with a sorted list this reffing doesn't work
        // see: https://referencesource.microsoft.com/#System/compmod/system/collections/generic/sortedlist.cs,1d4d1ee1c4b1f6f9
        for (var i = 0; i < this.ContainerA.Length; i++)
        {
            ref var a = ref this.ContainerA[i];
            ref var b = ref this.ContainerB[10];

            this.Service.Process(ref a, ref b);
        }
    }
}


public sealed class ServiceA
{

    public void Process(ref SComponentA a, ref SComponentB b)
    {

    }
}
