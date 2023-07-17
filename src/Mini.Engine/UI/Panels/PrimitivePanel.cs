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
    private readonly CurveManager CurveManager;

    public string Title => "Primitives";

    private bool shouldReload;

    private ICurve lastCurve;
    private Matrix4x4 lastTransform;

    public PrimitivePanel(TrackManager trackManager, CurveManager curveManager)
    {
        this.TrackManager = trackManager;
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

        this.shouldReload = false;
    }

    private (Vector3 Position, Vector3 Forward) GetNextOrientation()
    {
        var (position, forward) = this.lastCurve.GetWorldOrientation(1.0f, this.lastTransform);        
        return (position + (forward * 0.1f), forward);
    }
}
