using System.Numerics;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics.Transforms;
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
        var front = this.Administrator.Entities.Create();
        var rear = this.Administrator.Entities.Create();
        var car = this.Administrator.Entities.Create();

        var trainCar = new TrainCar(front, rear, car, name);

        var bogiePrimitive = TrainCars.BuildBogie(device, nameof(front));        
        //var carPrimitive = TrainCars.BuildBogie(device, nameof(front));

        this.CreateComponents(device, front, bogiePrimitive, 0.1f);
        this.CreateComponents(device, rear, bogiePrimitive, 0.1f);
        //this.CreateComponents(car, carPrimitive);

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

    private void CreateComponents(Device device, Entity entity, ILifetime<PrimitiveMesh> mesh, float shadowImportance)
    {
        var components = this.Administrator.Components;

        ref var transform = ref components.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var primitive = ref components.Create<PrimitiveComponent>(entity);
        primitive.Mesh = mesh;

        ref var shadows = ref components.Create<ShadowCasterComponent>(entity);
        shadows.Importance = shadowImportance;

        ref var instances = ref components.Create<InstancesComponent>(entity);
        instances.Init(device, $"Instances{entity}", BufferCapacity);
    }
}
