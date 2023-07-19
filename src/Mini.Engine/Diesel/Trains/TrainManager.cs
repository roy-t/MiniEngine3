using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.Diesel.Trains;

[Service]
public sealed class TrainManager
{
    private const int BufferCapacity = 25;

    private readonly ECSAdministrator Administrator;
    private readonly TrackGrid Grid;
    private readonly IComponentContainer<InstancesComponent> Instances;
    private readonly TrainCar FlatCar;

    public TrainManager(Device device, ScenarioManager scenarioManager, ECSAdministrator administrator, IComponentContainer<InstancesComponent> instances)
    {
        this.Grid = scenarioManager.Grid;
        this.Administrator = administrator;

        this.FlatCar = this.CreateTrainCarAndComponents(device, nameof(this.FlatCar));
        this.Instances = instances;
    }

    private TrainCar CreateTrainCarAndComponents(Device device, string name)
    {
        var bogiePrimitive = TrainCars.BuildBogie(device, "bogie");
        //var carPrimitive = TrainCars.BuildBogie(device, nameof(front));

        var front = PrimitiveUtilities.CreateComponents(device, this.Administrator, bogiePrimitive, BufferCapacity, 1.0f);
        var rear = PrimitiveUtilities.CreateComponents(device, this.Administrator, bogiePrimitive, BufferCapacity, 1.0f);
        var car = this.Administrator.Entities.Create();

        var trainCar = new TrainCar(front, rear, car, name);

        return trainCar;
    }

    public void AddFlatCar(Vector3 approximatePosition)
    {
        var (x, y) = this.Grid.PickCell(approximatePosition);
        var placement = this.Grid[x, y].Placements[0];

        this.AddInstance(this.FlatCar.Front, placement.Curve, 0.5f, placement.Transform);

        // TODO: add a find distance function
        this.AddInstance(this.FlatCar.Rear, placement.Curve, 0.6f, placement.Transform);
    }

    private void AddInstance(Entity entity, ICurve curve, float u, Transform transform)
    {
        ref var component = ref this.Instances[entity];
        var matrix = curve.AlignTo(u, Vector3.UnitY, in transform);
        component.Value.InstanceList.Add(matrix);
        component.LifeCycle = component.LifeCycle.ToChanged();
    }
}
