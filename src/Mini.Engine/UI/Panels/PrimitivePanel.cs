using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Diesel;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.Diesel.Trains;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class PrimitivePanel : IEditorPanel
{
    private readonly Device Device;
    private readonly ECSAdministrator Administrator;
    private readonly TrackManager TrackManager;
    private readonly TrainManager TrainManager;
    private readonly CurveManager CurveManager;

    public string Title => "Primitives";

    private bool shouldReload;

    private Entity lastEntity;
    private ICurve lastCurve;
    private Matrix4x4 lastTransform;

    public PrimitivePanel(Device device, ECSAdministrator administrator, TrackManager trackManager, TrainManager trainManager, CurveManager curveManager)
    {
        this.Device = device;
        this.Administrator = administrator;
        this.TrackManager = trackManager;
        this.TrainManager = trainManager;
        this.CurveManager = curveManager;
        this.lastCurve = curveManager.Straight;
        this.lastTransform = Matrix4x4.Identity;

#if DEBUG
        HotReloadManager.AddReloadCallback("Mini.Engine.Modelling", _ => this.shouldReload = true);
        HotReloadManager.AddReloadCallback("Mini.Engine.Diesel", _ => this.shouldReload = true);
        HotReloadManager.AddReloadCallback("Mini.Engine.UI.Panels.PrimitivePanel", _ => this.shouldReload = true);
#endif
    }

    public void Update(float elapsed)
    {
        if (ImGui.Button("Clear"))
        {
            this.TrackManager.Clear();
            this.Administrator.Components.MarkForRemoval(this.lastEntity);

            this.lastEntity = new Entity(0);
            this.lastCurve = this.CurveManager.Straight;
            this.lastTransform = Matrix4x4.Identity;                        
        }

        if (ImGui.Button("Forward"))
        {
            var (position, forward) = this.GetNextOrientation();
            (this.lastTransform, this.lastCurve) = this.TrackManager.AddStraight(position, forward);
        }

        if (ImGui.Button("Turn Left"))
        {
            var (position, forward) = this.GetNextOrientation();
            (this.lastTransform, this.lastCurve) = this.TrackManager.AddLeftTurn(position, forward);
        }

        if (ImGui.Button("Turn Right"))
        {
            var (position, forward) = this.GetNextOrientation();
            (this.lastTransform, this.lastCurve) = this.TrackManager.AddRightTurn(position, forward);
        }

        if (ImGui.Button("Add Train"))
        {
            var position = Vector3.Transform(this.lastCurve.GetPosition(0.5f), this.lastTransform);
            this.TrainManager.AddFlatCar(position);
        }

        if (ImGui.Button("Generate"))
        {
            var primitive = TrainCars.BuildFlatCar(this.Device, "flatcar");
            this.lastEntity = PrimitiveUtilities.CreateComponents(this.Device, this.Administrator, primitive, 1, 1.0f);

            ref var instances = ref this.Administrator.Components.GetComponent<InstancesComponent>(this.lastEntity);
            instances.Value.InstanceList.Add(Matrix4x4.Identity);
        }

        this.shouldReload = false;
    }

    private (Vector3 Position, Vector3 Forward) GetNextOrientation()
    {
        var (position, forward) = this.lastCurve.GetWorldOrientation(1.0f, in this.lastTransform);
        return (position + (forward * 0.1f), forward);
    }
}
