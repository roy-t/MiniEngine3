using System.Numerics;
using ImGuiNET;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.DirectX;
using Mini.Engine.DirectX.Buffers;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics.Lines;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Diesel.Trains;

namespace Mini.Engine.UI.Panels;

[Service]
internal class PrimitivePanel : IEditorPanel
{
    private readonly Device Device;
    private readonly IComponentContainer<PrimitiveComponent> Container;
    private readonly ComponentSelector<PrimitiveComponent> Primitives;
    private readonly ECSAdministrator Administrator;

    public string Title => "Primitives";

    private bool shouldReload;

    public PrimitivePanel(Device device, IComponentContainer<PrimitiveComponent> container, ECSAdministrator administrator)
    {
        this.Device = device;
        this.Container = container;
        this.Primitives = new ComponentSelector<PrimitiveComponent>("Primitives", container);
        this.Administrator = administrator;

#if DEBUG
        HotReloadManager.AddReloadCallback("Mini.Engine.Modelling", _ => this.shouldReload = true);
        HotReloadManager.AddReloadCallback("Mini.Engine.Diesel", _ => this.shouldReload = true);
        HotReloadManager.AddReloadCallback("Mini.Engine.UI.Panels.PrimitivePanel", _ => this.shouldReload = true);
#endif
    }

    public void Update(float elapsed)
    {
        this.Primitives.Update();
        if (ImGui.Button("Add"))
        {
            this.CreatePrimitives();
        }

        if (this.Primitives.HasComponent())
        {
            ImGui.SameLine();
            if (ImGui.Button("Remove"))
            {
                ref var component = ref this.Primitives.Get();
                this.Administrator.Components.MarkForRemoval(component.Entity);
            }

            ImGui.SameLine();
            if (ImGui.Button("Clear"))
            {
                this.ClearPrimitives();
            }
        }

        if (this.shouldReload)
        {
            this.ClearPrimitives();

            this.shouldReload = false;
            this.CreatePrimitives();
        }
    }

    private void ClearPrimitives()
    {
        foreach (var entity in this.Container.IterateAllEntities())
        {
            this.Administrator.Components.MarkForRemoval(entity);
        }
    }

    private void CreatePrimitives()
    {
        var straight = TrackPieces.Straight(this.Device);
        this.CreateVisuals(straight, "straight");

        //var turn = TrackPieces.Turn(this.Device);
        //this.CreateVisuals(turn, "turn");

        var flatcar = TrainCars.Flatcar(this.Device);
        this.CreateVisuals(flatcar, "flatcar");
    }

    private void CreateVisuals(TrainCar car, string name)
    {
        var entity = this.Administrator.Entities.Create();
        this.CreateVisuals(entity, car.Mesh, name, car.Offset.GetMatrix());
    }

    private void CreateVisuals(TrackPiece piece, string name)
    {        
        var entity = this.Administrator.Entities.Create();
        this.CreateVisuals(entity, piece.Mesh, name, Matrix4x4.Identity);
        this.CreateVisuals(entity, piece.Curve, piece.Bounds, name);
    }

    private void CreateVisuals(Entity entity, ICurve curve, BoundingBox bounds, string name)
    {
        var creator = this.Administrator.Components;

        ref var line = ref creator.Create<LineComponent>(entity);
        var lineVertices = curve.GetPoints3D(50, new Vector3(0.0f, bounds.Max.Y, 0.0f));
        var mesh = new LineMesh(this.Device, $"{name}_line", lineVertices);
        line.Mesh = this.Device.Resources.Add(mesh);
        line.Color = Colors.Yellow;
    }

    private void CreateVisuals(Entity entity, ILifetime<PrimitiveMesh> mesh, string name, params Matrix4x4[] matrices)
    {
        var creator = this.Administrator.Components;    

        ref var instances = ref creator.Create<InstancesComponent>(entity);
        instances.InstanceBuffer = this.Instance($"{name}_instances", matrices);
        instances.InstanceCount = matrices.Length;

        ref var transform = ref creator.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var component = ref creator.Create<PrimitiveComponent>(entity);
        component.Mesh = mesh;

        ref var shadowCaster = ref creator.Create<ShadowCasterComponent>(entity);
        shadowCaster.Importance = 0.0f;
    }

    private ILifetime<StructuredBuffer<Matrix4x4>> Instance(string name, params Matrix4x4[] instances)
    {
        var buffer = new StructuredBuffer<Matrix4x4>(this.Device, name);
        buffer.MapData(this.Device.ImmediateContext, instances);

        return this.Device.Resources.Add(buffer);
    }
}
