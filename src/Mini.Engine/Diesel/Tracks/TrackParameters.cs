using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Tracks;
public static class TrackParameters
{
    public static Color3 RAIL_COLOR => new(0.4f, 0.28f, 0.30f);
    public static float RAIL_METALICNESS => 0.6f;
    public static float RAIL_ROUGHNESS => 0.05f;

    public static Color3 RAIL_TIE_COLOR => new(0.4f, 0.4f, 0.4f);
    public static float RAIL_TIE_METALICNESS => 0.1f;
    public static float RAIL_TIE_ROUGHNESS => 0.8f;

    public static Color3 BALLAST_COLOR => new(0.33f, 0.27f, 0.25f);
    public static float BALLAST_METALICNESS => 0.0f;
    public static float BALLAST_ROUGHNESS => 1.0f;

    public static int TURN_RADIUS => 25;
    public static int TURN_VERTICES => 25;

    public static int STRAIGHT_VERTICES => 2;
    public static float STRAIGHT_LENGTH => 50.0f;

    public static float SINGLE_RAIL_WIDTH_TOP => 0.1f;
    public static float SINGLE_RAIL_WIDTH_BOTTOM => 0.2f;
    public static float SINGLE_RAIL_HEIGTH => 0.2f;
    public static float SINGLE_RAIL_OFFSET => 0.65f + (SINGLE_RAIL_WIDTH_BOTTOM / 2.0f);
    public static float BALLAST_WIDTH_TOP => 3.0f;
    public static float BALLAST_WIDTH_MIDDLE => 4.0f;
    public static float BALLAST_WIDTH_BOTTOM => 6.0f;
    public static float BALLAST_HEIGHT_TOP => 1.0f;
    public static float BALLAST_HEIGHT_MIDDLE => 0.5f;
    public static float BALLAST_HEIGHT_BOTTOM => 0.0f;
    public static float RAIL_TIE_WIDTH => 2.0f;
    public static float RAIL_TIE_WIDTH_BOTTOM => 2.2f;
    public static float RAIL_TIE_HEIGHT => 0.15f;
    public static float RAIL_TIE_MID_HEIGHT => 0.05f;
    public static float RAIL_TIE_DEPTH_TOP => 0.2f;
    public static float RAIL_TIE_DEPTH_BOTTOM => 0.3f;
    public static float RAIL_TIE_SPACING => 0.3f + RAIL_TIE_DEPTH_BOTTOM;
}
