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
        var carPrimitive = TrainCars.BuildFlatCar(device, "flat_car");

        var front = PrimitiveUtilities.CreateComponents(device, this.Administrator, bogiePrimitive, BufferCapacity, 1.0f);
        var rear = PrimitiveUtilities.CreateComponents(device, this.Administrator, bogiePrimitive, BufferCapacity, 1.0f);
        var car = PrimitiveUtilities.CreateComponents(device, this.Administrator, carPrimitive, BufferCapacity, 1.0f);

        var trainCar = new TrainCar(front, rear, car, name);

        return trainCar;
    }

    public void AddFlatCar(Vector3 approximatePosition)
    {
        var (x, y) = this.Grid.PickCell(approximatePosition);
        var placement = this.Grid[x, y].Placements[0];

        var (positionBack, _) = this.AddInstance(this.FlatCar.Front, placement.Curve, 0.1f, placement.Transform);

        // TODO: add a function that finds the position X world distance further
        // look at SegmentedCurve for an easy way to do it
        // note that since SegmentedCurve only looks at distance along the line, before applying the transform
        // it doesn't really matter with symetric pieces what way the curves are pointing
        // though it would probably be best if we skip that hole problem by reversing curves
        // in the grid so that we make 2 sides connections
        // and only count the 1->0 connections as connected (might also make it possible for 1 way tracks?)
        var (positionFront, _) = this.AddInstance(this.FlatCar.Rear, placement.Curve, 0.6f, placement.Transform);

        var carMatrix = Transform.Identity.SetTranslation((positionBack + positionFront) * 0.5f)
            .FaceTargetConstrained(positionBack + Vector3.Normalize(positionFront - positionBack), Vector3.UnitY);
        this.AddInstance(this.FlatCar.Car, carMatrix.GetMatrix());
    }

    private (Vector3 Position, Vector3 Forward) AddInstance(Entity entity, ICurve curve, float u, Transform transform)
    {        
        var matrix = curve.AlignTo(u, Vector3.UnitY, in transform);
        this.AddInstance(entity, in matrix);

        return curve.GetWorldOrientation(u, transform.GetMatrix());
    }

    private void AddInstance(Entity entity, in Matrix4x4 matrix)
    {
        ref var component = ref this.Instances[entity];
        component.Value.InstanceList.Add(matrix);
        component.LifeCycle = component.LifeCycle.ToChanged();
    }
}
