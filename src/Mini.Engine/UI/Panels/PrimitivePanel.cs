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
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;

namespace Mini.Engine.UI.Panels;

[Service]
internal class PrimitivePanel : IDieselPanel, IEditorPanel
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
        var turn = TrackPieces.Turn(this.Device);
        this.CreateAll("turn", turn);
    }

    private void CreateAll(string name, TrackPiece piece)
    {
        var entity = this.Administrator.Entities.Create();
        var creator = this.Administrator.Components;

        var matrices = new Matrix4x4[]
        {
           Matrix4x4.Identity
        };
        
        ref var instances = ref creator.Create<InstancesComponent>(entity);
        instances.InstanceBuffer = Instance($"{name}_instances", matrices);
        instances.InstanceCount = matrices.Length;

        ref var transform = ref creator.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var component = ref creator.Create<PrimitiveComponent>(entity);
        component.Mesh = piece.Mesh;

        ref var line = ref creator.Create<LineComponent>(entity);
        var lineVertices = piece.Curve.GetPoints3D(50, new Vector3(0.0f, piece.Bounds.Max.Y, 0.0f));
        var mesh = new LineMesh(this.Device, $"{name}_line", lineVertices);
        line.Mesh = this.Device.Resources.Add(mesh);
        line.Color = Colors.Yellow;
    }


    private ILifetime<StructuredBuffer<Matrix4x4>> Instance(string name, params Matrix4x4[] instances)
    {
        var buffer = new StructuredBuffer<Matrix4x4>(this.Device, name);
        buffer.MapData(this.Device.ImmediateContext, instances);

        return this.Device.Resources.Add(buffer);
    }
}
