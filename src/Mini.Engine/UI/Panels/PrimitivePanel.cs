using System.Numerics;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class PrimitivePanel : IEditorPanel
{
    private readonly TrackManager TrackManager;
    private readonly TrackComputer TrackComputer;

    public string Title => "Primitives";

    private bool shouldReload;

    private Vector3 offset;
    private Vector3 forward;

    public PrimitivePanel(TrackManager trackManager, TrackComputer trackComputer)
    {
        this.TrackManager = trackManager;
        this.TrackComputer = trackComputer;

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
            this.TrackManager.AddStraight(placement.Id, placement.Transform.GetMatrix());
        }

        if (ImGui.Button("Turn Left"))
        {
            var placement = this.PlaceTrack(this.TrackManager.LeftTurn.Curve);
            this.TrackManager.AddLeftTurn(placement.Id, placement.Transform.GetMatrix());
        }

        if (ImGui.Button("Turn Right"))
        {
            var placement = this.PlaceTrack(this.TrackManager.RightTurn.Curve);
            this.TrackManager.AddRightTurn(placement.Id, placement.Transform.GetMatrix());
        }

        this.shouldReload = false;
    }

    private CurvePlacement PlaceTrack(ICurve curve)
    {
        var placement = this.TrackComputer.PlaceCurve(this.offset, this.forward, curve);

        this.offset = placement.EndPosition;
        this.forward = placement.EndForward;

        return placement;
    }
}
