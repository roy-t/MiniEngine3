using System.Numerics;
using ImGuiNET;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Diesel.Tracks;
using LibGame.Mathematics;
using Mini.Engine.Modelling.Curves;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class PrimitivePanel : IEditorPanel
{
    private readonly TrackManager TrackManager;

    public string Title => "Primitives";

    private bool shouldReload;

    private Vector3 Offset;
    private float Angle;


    private Vector3 offset = new Vector3(0,0, 0);
    private Vector3 forward = new Vector3(0, 0, -1);

    public PrimitivePanel(TrackManager trackManager)
    {
        this.TrackManager = trackManager;

#if DEBUG
        HotReloadManager.AddReloadCallback("Mini.Engine.Modelling", _ => this.shouldReload = true);
        HotReloadManager.AddReloadCallback("Mini.Engine.Diesel", _ => this.shouldReload = true);
        HotReloadManager.AddReloadCallback("Mini.Engine.UI.Panels.PrimitivePanel", _ => this.shouldReload = true);
#endif
    }

    public void Update(float elapsed)
    {
        if(ImGui.Button("JUST"))
        {
            this.TrackManager.Just();            
        }

        // TODO: the info the get the start and end and correct rotation
        // should probably be stored somewhere else
        // ideally you could select a piece of track, and then choose an entry/exit and then add another valid piece to it
        if (ImGui.Button("Forward"))
        {
            var curve = this.TrackManager.Straight.Curve;
            var add = this.TrackManager.AddStraight;
            this.AddStuff(curve, add);

            //var transform = new Transform(this.Offset, Quaternion.CreateFromYawPitchRoll(this.Angle, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, TrackParameters.STRAIGHT_LENGTH * 0.5f), 1.0f);
            //this.TrackManager.AddStraight(transform.GetMatrix());            
            //this.Offset += Vector3.Transform(new Vector3(0, 0, -TrackParameters.STRAIGHT_LENGTH), transform.GetRotation());
        }

        if (ImGui.Button("Turn Left"))
        {
            var curve = this.TrackManager.Turn.Curve;
            var add = this.TrackManager.AddTurn;
            this.AddStuff(curve, add);

            //var transform = new Transform(this.Offset, Quaternion.CreateFromYawPitchRoll(this.Angle, 0.0f, 0.0f), new Vector3(TrackParameters.TURN_RADIUS, 0.0f, 0.0f), 1.0f);
            //this.TrackManager.AddTurn(transform.GetMatrix());
            //this.Offset += Vector3.Transform(new Vector3(-TrackParameters.TURN_RADIUS, 0, -TrackParameters.TURN_RADIUS), transform.GetRotation());
            //this.Angle = Radians.WrapRadians(this.Angle + (MathF.PI * 0.5f));
        }

        if (ImGui.Button("Turn Right"))
        {
            var transform = new Transform(this.Offset, Quaternion.CreateFromYawPitchRoll(this.Angle + (MathF.PI * 0.5f), 0.0f, 0.0f), new Vector3(0.0f, 0.0f, -TrackParameters.TURN_RADIUS), 1.0f);
            this.TrackManager.AddTurn(transform.GetMatrix());
            this.Offset += Vector3.Transform(new Vector3(TrackParameters.TURN_RADIUS, 0, -TrackParameters.TURN_RADIUS), Quaternion.CreateFromYawPitchRoll(this.Angle, 0.0f, 0.0f));
            this.Angle = Radians.WrapRadians(this.Angle - (MathF.PI * 0.5f));
        }

        if (ImGui.Button("Clear"))
        {
            this.TrackManager.Clear();
            this.Offset = Vector3.Zero;
            this.Angle = 0.0f;

            this.offset = Vector3.Zero;
            this.forward = new Vector3(0, 0, -1);
        }
        
        //if (this.shouldReload)
        //{
        //  create some default track
        //}
    }

    private void AddStuff(ICurve curve, Action<Matrix4x4> add)
    {        
        var orientation = TrackComputer.PlaceCurve(this.offset, this.forward, curve, 0.0f);
        var transform = new Transform(orientation.Offset, Quaternion.CreateFromYawPitchRoll(orientation.Yaw, 0.0f, 0.0f), Vector3.Zero, 1.0f);
        add(transform.GetMatrix());

        var curveOffset = curve.GetPosition(1.0f) - curve.GetPosition(0);
        this.offset += Vector3.Transform(curveOffset, transform.GetRotation());

        var f = Vector3.Transform(curve.GetForward(1.0f), transform.GetRotation());
        this.forward = f;
    }
}
