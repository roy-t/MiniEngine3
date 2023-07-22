using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Content;
using Mini.Engine.Content.Models;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;
using static Mini.Engine.Diesel.Tracks.TrackParameters;
namespace Mini.Engine.Diesel.Trains;

[Service]
public sealed class TrainManager
{
    private const int BufferCapacity = 25;

    private readonly ECSAdministrator Administrator;
    private readonly TrackGrid Grid;
    private readonly IComponentContainer<InstancesComponent> Instances;
    private readonly TrainCar FlatCar;

    public TrainManager(Device device, ContentManager content, ScenarioManager scenarioManager, ECSAdministrator administrator, IComponentContainer<InstancesComponent> instances)
    {
        this.Grid = scenarioManager.Grid;
        this.Administrator = administrator;

        this.FlatCar = this.CreateTrainCarAndComponents(device, content, nameof(this.FlatCar));
        this.Instances = instances;                
    }

    private TrainCar CreateTrainCarAndComponents(Device device, ContentManager content, string name)
    {
        //var bogiePrimitive = TrainCars.BuildBogie(device, "bogie");        
        var bogiePrimitive = TrainCars.BuildBogie(device, content, "bogie");
        var bogieBounds = device.Resources.Get(bogiePrimitive).Bounds;
        var carPrimitive = TrainCars.BuildFlatCar(device, content, "flat_car", in bogieBounds);

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

        if (!placement.Curve.TravelEucledianDistance(0.1f, TrainParameters.FLAT_CAR_BOGEY_CENTER_DISTANCE, 0.01f, out var uEnd))
        {
            uEnd = 1.0f;
        }
        var (positionFront, _) = this.AddInstance(this.FlatCar.Rear, placement.Curve, uEnd, placement.Transform);

        var carMatrix = Transform.Identity
            .SetTranslation(Vector3.Lerp(positionBack, positionFront, 0.5f))
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
