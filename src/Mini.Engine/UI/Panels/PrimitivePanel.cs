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
        // TODO: use transform with offset center to place these things as
        // using a normal rotation matrix will just do it from the center
        var transform = Transform.Identity;

        var rotation = Matrix4x4.CreateRotationY(this.Angle);
        var translation = Matrix4x4.CreateTranslation(this.Offset);

        if (ImGui.Button("Forward"))
        {            
            this.TrackManager.AddStraight(translation * rotation);
            this.Offset +=  Vector3.Transform(new Vector3(0, 0, -TrackParameters.STRAIGHT_LENGTH), rotation);
        }

        if (ImGui.Button("Turn Left"))
        {            
            this.TrackManager.AddTurn(translation * rotation);
            this.Offset += new Vector3(-TrackParameters.TURN_RADIUS, 0, -TrackParameters.TURN_RADIUS);
            //this.Angle += MathF.PI * 0.5f;
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
