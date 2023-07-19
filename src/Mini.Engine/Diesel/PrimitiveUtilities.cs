using LibGame.Physics;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Diesel;
public static class PrimitiveUtilities
{
    public static Entity CreateComponents(Device device, ECSAdministrator administator, ILifetime<PrimitiveMesh> mesh, int instanceBufferCapacity, float shadowImportance)
    {
        var entity = administator.Entities.Create();
        var components = administator.Components;

        ref var transform = ref components.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var primitive = ref components.Create<PrimitiveComponent>(entity);
        primitive.Mesh = mesh;

        if (shadowImportance > 0)
        {
            ref var shadows = ref components.Create<ShadowCasterComponent>(entity);
            shadows.Importance = shadowImportance;
        }

        ref var instances = ref components.Create<InstancesComponent>(entity);
        instances.Init(device, $"Instances{entity}", instanceBufferCapacity);


        return entity;
    }
}
