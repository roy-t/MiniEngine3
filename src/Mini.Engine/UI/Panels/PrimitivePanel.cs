using System.Numerics;
using ImGuiNET;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class PrimitivePanel : IEditorPanel
{
    private readonly TrackManager TrackManager;

    public string Title => "Primitives";

    private bool shouldReload;

    private Vector3 offset;
    private Vector3 forward;

    public PrimitivePanel(TrackManager trackManager)
    {
        this.TrackManager = trackManager;

        this.offset = Vector3.Zero;
        this.forward = new Vector3(0, 0, -1);

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

            this.offset = Vector3.Zero;
            this.forward = new Vector3(0, 0, -1);
        }

        if (ImGui.Button("Forward") || this.shouldReload)
        {
            var placement = this.PlaceTrack(this.TrackManager.Straight.Curve);
            this.TrackManager.AddStraight(0, placement.GetMatrix());
        }

        if (ImGui.Button("Turn Left"))
        {
            var placement = this.PlaceTrack(this.TrackManager.LeftTurn.Curve);
            this.TrackManager.AddLeftTurn(0, placement.GetMatrix());
        }

        if (ImGui.Button("Turn Right"))
        {
            var placement = this.PlaceTrack(this.TrackManager.RightTurn.Curve);
            this.TrackManager.AddRightTurn(0, placement.GetMatrix());
        }

        this.shouldReload = false;
    }

    private Transform PlaceTrack(ICurve curve)
    {
        var transform = curve.PlaceInXZPlane(0.0f, this.offset, this.forward);
        (this.offset, this.forward) = curve.GetWorldOrientation(1.0f, transform);

        return transform;
    }
}
