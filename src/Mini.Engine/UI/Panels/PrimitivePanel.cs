using System.Numerics;
using ImGuiNET;
using LibGame.Physics;
using Mini.Engine.Configuration;
using Mini.Engine.Core.Lifetime;
using Mini.Engine.Diesel.Tracks;
using Mini.Engine.DirectX;
using Mini.Engine.ECS;
using Mini.Engine.Graphics.Primitives;
using Mini.Engine.Graphics.Lines;
using Mini.Engine.Graphics.Transforms;
using Mini.Engine.Modelling.Curves;
using Vortice.Mathematics;
using Mini.Engine.Graphics.Lighting.ShadowingLights;
using Mini.Engine.Diesel.Trains;
using LibGame.Mathematics;

namespace Mini.Engine.UI.Panels;

[Service]
internal sealed class PrimitivePanel : IEditorPanel
{
    private readonly TrackManager TrackManager;

    public string Title => "Primitives";

    private bool shouldReload;

    private Vector3 Offset;
    private float Angle;

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
        // TODO: the info the get the start and end and correct rotation
        // should probably be stored somewhere else
        // ideally you could select a piece of track, and then choose an entry/exit and then add another valid piece to it
        if (ImGui.Button("Forward"))
        {
            var transform = new Transform(this.Offset, Quaternion.CreateFromYawPitchRoll(this.Angle, 0.0f, 0.0f), new Vector3(0.0f, 0.0f, TrackParameters.STRAIGHT_LENGTH * 0.5f), 1.0f);
            this.TrackManager.AddStraight(transform.GetMatrix());            
            this.Offset += Vector3.Transform(new Vector3(0, 0, -TrackParameters.STRAIGHT_LENGTH), transform.GetRotation());
        }

        if (ImGui.Button("Turn Left"))
        {
            var transform = new Transform(this.Offset, Quaternion.CreateFromYawPitchRoll(this.Angle, 0.0f, 0.0f), new Vector3(TrackParameters.TURN_RADIUS, 0.0f, 0.0f), 1.0f);
            this.TrackManager.AddTurn(transform.GetMatrix());
            this.Offset += Vector3.Transform(new Vector3(-TrackParameters.TURN_RADIUS, 0, -TrackParameters.TURN_RADIUS), transform.GetRotation());
            this.Angle = Radians.WrapRadians(this.Angle + (MathF.PI * 0.5f));
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
        }
        
        //if (this.shouldReload)
        //{
        //  create some default track
        //}
    }
   
}
