using System.Numerics;
using Mini.Engine.ECS;

namespace Mini.Engine.Diesel.Trains;
public sealed class TrainCar
{
    public TrainCar(Entity front, Entity rear, Entity car, string name)
    {
        this.Front = front;
        this.Rear = rear;
        this.Car = car;
        this.Name = name;
        this.FrontInstances = new List<Matrix4x4>();
        this.RearInstances = new List<Matrix4x4>();
        this.CarInstances = new List<Matrix4x4>();
    }

    public Entity Front { get; }
    public List<Matrix4x4> FrontInstances { get; }

    public Entity Rear { get; }
    public List<Matrix4x4> RearInstances { get; }

    public Entity Car { get; }
    public List<Matrix4x4> CarInstances { get; }

    public string Name { get; }    
}
