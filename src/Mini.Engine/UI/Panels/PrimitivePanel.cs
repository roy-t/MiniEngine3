﻿using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Modelling;
using Mini.Engine.Modelling.Generators;
using Vortice.Mathematics;

namespace Mini.Engine.UI.Panels;

[Service]
internal class PrimitivePanel : IDieselPanel
{
    
    private readonly IComponentContainer<PrimitiveComponent> Container;
    private readonly ComponentSelector<PrimitiveComponent> Primitives;
    private readonly ECSAdministrator Administrator;
    private readonly PrimitiveMeshBuilder Builder;

    public string Title => "Primitives";

    private bool shouldReload;

    public PrimitivePanel(IComponentContainer<PrimitiveComponent> container, ECSAdministrator administrator, PrimitiveMeshBuilder builder)
    {
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
        }

        if (this.shouldReload)
        {
            foreach (var entity in this.Container.IterateAllEntities())
            {
                this.Administrator.Components.MarkForRemoval(entity);
            }

            this.shouldReload = false;
            this.CreatePrimitives();
        }
    }

    private void CreatePrimitives()
    {
        var trackLayout = TrainRailGenerator.CreateTrackLayout();
        this.CreateRailPrimitive(trackLayout);
        this.CreateBallastPrimitive(trackLayout);

    }

    private void CreateRailPrimitive(Path3D trackLayout)
    {
        var entity = this.Administrator.Entities.Create();
        var creator = this.Administrator.Components;

        ref var transform = ref creator.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var component = ref creator.Create<PrimitiveComponent>(entity);

        var rails = TrainRailGenerator.GenerateRails(trackLayout);

        component.Mesh = this.Builder.FromQuads("rail", rails);
        component.Color = new Color4(0.4f, 0.28f, 0.30f, 1.0f);
    }

    private void CreateBallastPrimitive(Path3D trackLayout)
    {
        var entity = this.Administrator.Entities.Create();
        var creator = this.Administrator.Components;

        ref var transform = ref creator.Create<TransformComponent>(entity);
        transform.Current = Transform.Identity;
        transform.Previous = transform.Current;

        ref var component = ref creator.Create<PrimitiveComponent>(entity);

        var ballast = TrainRailGenerator.GenerateBallast(trackLayout);

        component.Mesh = this.Builder.FromQuads("ballast", ballast);
        component.Color = new Color4(0.33f, 0.27f, 0.25f, 1.0f);
    }
}