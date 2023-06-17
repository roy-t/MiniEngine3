using System.Numerics;
using System.Runtime.InteropServices;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics.Transforms;

namespace Mini.Engine.Diesel.Tracks;

[Service]
public sealed class TrackManager
{
    private sealed record class TrackPieceLayout(Entity Entity, TrackPiece TrackPiece, List<Matrix4x4> Positions, bool IsDirty);

    private readonly Device Device;
    private readonly ECSAdministrator Administrator;
    private readonly IComponentContainer<PrimitiveComponent> Primitives;
    private readonly IComponentContainer<ShadowCasterComponent> Shadows;
    private readonly IComponentContainer<TransformComponent> Transforms;
    private readonly IComponentContainer<InstancesComponent> Instances;

    private readonly List<TrackPieceLayout> Layouts;

    public TrackManager(Device device, ECSAdministrator administrator, IComponentContainer<PrimitiveComponent> primitives, IComponentContainer<ShadowCasterComponent> shadows, IComponentContainer<InstancesComponent> instances)
    {
        this.Device = device;
        this.Administrator = administrator;
        this.Primitives = primitives;
        this.Shadows = shadows;
        this.Instances = instances;

        this.Layouts = new List<TrackPieceLayout>();
    }

    public void Update()
    {
        var context = this.Device.ImmediateContext;

        foreach (var layout in this.Layouts)
        {
            if (layout.IsDirty)
            {
                var entity = layout.Entity;
                if (!this.Instances.Contains(entity))
                {
                    var components = this.Administrator.Components;
                    ref var instances = ref components.Create<InstancesComponent>(entity);
                    instances.Init(this.Device, layout.TrackPiece.Name, CollectionsMarshal.AsSpan(layout.Positions));
                }
                else
                {
                    ref var instances = ref this.Instances[layout.Entity].Value;
                    var buffer = context.Resources.Get(instances.InstanceBuffer);
                    buffer.MapData(context, CollectionsMarshal.AsSpan(layout.Positions));
                }

                layout.IsDirty = false;
            }
        }
    }

    // TODO: add a way to add an individual item to a Layout

    private void AddLayout(TrackPiece trackPiece)
    {
        var entities = this.Administrator.Entities;
        var components = this.Administrator.Components;

        var entity = entities.Create();

        ref var transform = ref components.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var primitive = ref components.Create<PrimitiveComponent>(entity);
        primitive.Mesh = trackPiece.Mesh;

        ref var shadows = ref components.Create<ShadowCasterComponent>(entity);
        shadows.Importance = 0.0f; // TODO: figure out which parts do and do not need a shadow
    }
}
