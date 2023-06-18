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
    private const int BufferCapacityIncrement = 100;

    private readonly Device Device;
    private readonly ECSAdministrator Administrator;
    private readonly IComponentContainer<InstancesComponent> Instances;

    private readonly List<TrackPiece> Pieces;

    private readonly TrackPiece Straight;
    private readonly TrackPiece Turn;

    public TrackManager(Device device, ECSAdministrator administrator, IComponentContainer<InstancesComponent> instances)
    {
        this.Device = device;
        this.Administrator = administrator;
        this.Instances = instances;

        var entities = this.Administrator.Entities;

        this.Straight = TrackPieces.Straight(device, entities.Create());
        this.Turn = TrackPieces.Turn(device, entities.Create());

        this.Pieces = new List<TrackPiece>()
        {
            this.Straight,
            this.Turn
        };

        foreach (var trackPiece in this.Pieces)
        {
            this.CreateComponents(trackPiece);
        }
    }

    public void Update()
    {
        var context = this.Device.ImmediateContext;

        foreach (var piece in this.Pieces)
        {
            if (piece.IsDirty)
            {
                var entity = piece.Entity;
                if (!this.Instances.Contains(entity))
                {
                    var components = this.Administrator.Components;
                    ref var instances = ref components.Create<InstancesComponent>(entity);
                    instances.Init(this.Device, piece.Name, CollectionsMarshal.AsSpan(piece.Instances), BufferCapacityIncrement);
                }
                else
                {
                    ref var instances = ref this.Instances[piece.Entity].Value;
                    var buffer = context.Resources.Get(instances.InstanceBuffer);
                    if (buffer.Capacity < piece.Instances.Count)
                    {
                        buffer.EnsureCapacity(buffer.Capacity + BufferCapacityIncrement);
                    }
                    buffer.MapData(context, CollectionsMarshal.AsSpan(piece.Instances));
                    instances.InstanceCount = piece.Instances.Count;
                    //var view = context.Resources.Get(instances.InstanceBufferView);
                    //view.Dispose();
                    //instances.InstanceBufferView = context.Resources.Add(buffer.CreateShaderResourceView());
                }

                piece.IsDirty = false;
            }
        }
    }

    public void Clear()
    {
        foreach(var piece in this.Pieces)
        {
            piece.Instances.Clear();
            piece.IsDirty = true;
        }
    }

    public void AddStraight(Matrix4x4 offset)
    {
        AddInstance(this.Straight, offset);
    }

    public void AddTurn(Matrix4x4 offset)
    {
        AddInstance(this.Turn, offset);
    }

    private static void AddInstance(TrackPiece trackPiece, Matrix4x4 offset)
    {
        trackPiece.Instances.Add(offset);
        trackPiece.IsDirty = true;
    }

    private void CreateComponents(TrackPiece trackPiece)
    {
        var components = this.Administrator.Components;
        var entity = trackPiece.Entity;

        ref var transform = ref components.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var primitive = ref components.Create<PrimitiveComponent>(entity);
        primitive.Mesh = trackPiece.Mesh;

        ref var shadows = ref components.Create<ShadowCasterComponent>(entity);
        shadows.Importance = 0.0f; // TODO: figure out which parts do and do not need a shadow
    }
}
