using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.Diesel.Trains;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class PrimitivePanel : IEditorPanel
{
    private readonly TrackManager TrackManager;
    private readonly TrainManager TrainManager;
    private readonly CurveManager CurveManager;

    public string Title => "Primitives";

    private bool shouldReload;

    private ICurve lastCurve;
    private Matrix4x4 lastTransform;

    public PrimitivePanel(TrackManager trackManager, TrainManager trainManager, CurveManager curveManager)
    {
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
        if (ImGui.Button("Clear") || this.shouldReload)
        {
            this.TrackManager.Clear();

            this.lastCurve = this.CurveManager.Straight;
            this.lastTransform = Matrix4x4.Identity;
        }

        if (ImGui.Button("Forward") || this.shouldReload)
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

        this.shouldReload = false;
    }

    private (Vector3 Position, Vector3 Forward) GetNextOrientation()
    {
        var (position, forward) = this.lastCurve.GetWorldOrientation(1.0f, in this.lastTransform);
        return (position + (forward * 0.1f), forward);
    }
}
