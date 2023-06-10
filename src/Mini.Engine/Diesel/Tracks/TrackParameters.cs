using Vortice.Mathematics;

namespace Mini.Engine.Diesel.Tracks;
public static class TrackParameters
{
    public static Color4 RAIL_COLOR = new Color4(0.4f, 0.28f, 0.30f, 1.0f);
    public static Color4 RAIL_TIE_COLOR = new Color4(0.4f, 0.4f, 0.4f, 1.0f);
    public static Color4 BALLAST_COLOR = new Color4(0.33f, 0.27f, 0.25f, 1.0f);    

    public const float SINGLE_RAIL_WIDTH_TOP = 0.1f;
    public const float SINGLE_RAIL_WIDTH_BOTTOM = 0.2f;
    public const float SINGLE_RAIL_HEIGTH = 0.2f;
    public const float SINGLE_RAIL_OFFSET = 0.65f + (SINGLE_RAIL_WIDTH_BOTTOM / 2.0f);
    public const float BALLAST_WIDTH_TOP = 3.0f;
    public const float BALLAST_WIDTH_MIDDLE = 4.0f;
    public const float BALLAST_WIDTH_BOTTOM = 6.0f;
    public const float BALLAST_HEIGHT_TOP = 1.0f;
    public const float BALLAST_HEIGHT_MIDDLE = 0.5f;
    public const float BALLAST_HEIGHT_BOTTOM = 0.0f;
    public const float RAIL_TIE_WIDTH = 2.0f;
    public const float RAIL_TIE_WIDTH_BOTTOM = 2.2f;
    public const float RAIL_TIE_HEIGHT = 0.15f;
    public const float RAIL_TIE_MID_HEIGHT = 0.05f;
    public const float RAIL_TIE_DEPTH_TOP = 0.2f;
    public const float RAIL_TIE_DEPTH_BOTTOM = 0.3f;
    public const float RAIL_TIE_SPACING = 0.3f + RAIL_TIE_DEPTH_BOTTOM;
}
