using System.Numerics;
using LibGame.Physics;
using Vortice.Mathematics;
using static Mini.Engine.Diesel.Tracks.TrackParameters;

namespace Mini.Engine.Diesel.Trains;
public static class TrainParameters
{

    public static int WHEEL_VERTICES => 20;
    public static float INNER_WHEEL_RADIUS => 1.1f / 2.0f;
    public static float OUTER_WHEEL_RADIUS => 0.9f / 2.0f;
    public static float INNER_WHEEL_THICKNESS => 0.05f;
    public static float OUTER_WHEEL_THICKNESS => 0.15f;
    public static float AXLE_RADIUS => 0.125f;
    public static float WHEEL_SPACING => 1.4f;

    public static Color4 BOGIE_COLOR => new(112, 96, 88);
    public static float BOGIE_METALICNESS => 0.6f;
    public static float BOGIE_ROUGHNESS => 0.1f;
    

    public static Transform WHEEL_TRANSFORM
    {
        get
        {            
            var h = SINGLE_RAIL_HEIGTH + BALLAST_HEIGHT_TOP + OUTER_WHEEL_RADIUS;
            return Transform.Identity.AddTranslation(new Vector3(0.0f, h, 0.0f))
                .AddLocalRotation(Quaternion.CreateFromAxisAngle(Vector3.UnitY, MathF.PI * 0.5f));
        }
    }

}
