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
    }

    public Entity Front { get; }
    public Entity Rear { get; }
    public Entity Car { get; }
    public string Name { get; }
}
