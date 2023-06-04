﻿using System.Numerics;
using ImGuiNET;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Modelling;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Generators;
using Vortice.Mathematics;

namespace Mini.Engine.UI.Panels;

[Service]
internal class PrimitivePanel : IDieselPanel
{
    private readonly Device Device;
    private readonly IComponentContainer<PrimitiveComponent> Container;
    private readonly ComponentSelector<PrimitiveComponent> Primitives;
    private readonly ECSAdministrator Administrator;
    private readonly QuadBuilder Builder;

    public string Title => "Primitives";

    private bool shouldReload;

    public PrimitivePanel(Device device, IComponentContainer<PrimitiveComponent> container, ECSAdministrator administrator, QuadBuilder builder)
    {
        this.Device = device;
        this.Container = container;
        this.Primitives = new ComponentSelector<PrimitiveComponent>("Primitives", container);
        this.Administrator = administrator;
        this.Builder = builder;

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
        //this.CreateAll("rail");

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
        instances.InstanceBuffer = this.Builder.Instance($"{name}_instances", matrices);
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

    private void CreateAll(string name)
    {
        var entity = this.Administrator.Entities.Create();
        var creator = this.Administrator.Components;

        var matrices = new Matrix4x4[]
        {
            Matrix4x4.CreateFromYawPitchRoll(MathF.PI * 0.0f, 0.0f, 0.0f),
            Matrix4x4.CreateFromYawPitchRoll(MathF.PI * 0.5f, 0.0f, 0.0f),
            Matrix4x4.CreateFromYawPitchRoll(MathF.PI * 1.0f, 0.0f, 0.0f),
            Matrix4x4.CreateFromYawPitchRoll(MathF.PI * 1.5f, 0.0f, 0.0f),
        };

        var builder = new PrimitiveMeshBuilder();
        var curve = TrainRailGenerator.CreateTurn(builder);        

        ref var instances = ref creator.Create<InstancesComponent>(entity);
        instances.InstanceBuffer = this.Builder.Instance($"{name}_instances", matrices);
        instances.InstanceCount = matrices.Length;

        ref var transform = ref creator.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var component = ref creator.Create<PrimitiveComponent>(entity);        
        component.Mesh = builder.Build(this.Device, name, out var bounds);

        ref var line = ref creator.Create<LineComponent>(entity);
        var lineVertices = curve.GetPoints3D(50, new Vector3(0.0f, bounds.Height, 0.0f));
        var mesh = new LineMesh(this.Device, $"{name}_line", lineVertices);
        line.Mesh = this.Device.Resources.Add(mesh);
        line.Color = Colors.Yellow;
    }  
}
