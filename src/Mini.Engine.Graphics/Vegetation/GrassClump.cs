using System.Numerics;

using GrassInstanceData = Mini.Engine.Content.Shaders.Generated.Grass.InstanceData;

namespace Mini.Engine.Graphics.Vegetation;
public sealed class GrassClump
{
    public GrassClump(Vector2 position, Vector3 tint, float rotation, float scale, Mode<Vector2> applyPosition, Mode<Vector3> applyTint, Mode<float> applyRotation, Mode<float> applyScale)
    {
        this.Position = position;
        this.Tint = tint;
        this.Rotation = rotation;
        this.Scale = scale;
        this.ApplyPosition = applyPosition;
        this.ApplyTint = applyTint;
        this.ApplyRotation = applyRotation;
        this.ApplyScale = applyScale;
    }

    public static GrassClump Default(Vector2 position, Vector3 tint, float rotation, float scale)
    {
        return new GrassClump(position, tint, rotation, scale,
            (c, b, d) => b,
            (c, b, d) => b,
            (c, b, d) => b,
            (c, b, d) => b);
    }

    public Vector2 Position { get; }
    public Vector3 Tint { get; }
    public float Rotation { get; }
    public float Scale { get; }
    public Mode<Vector2> ApplyPosition { get; set; }
    public Mode<Vector3> ApplyTint { get; set; }
    public Mode<float> ApplyRotation { get; set; }
    public Mode<float> ApplyScale { get; set; }

    public void Apply(ref GrassInstanceData data)
    {
        var flatPosition = new Vector2(data.Position.X, data.Position.Z);
        var distance = Vector2.Distance(flatPosition, this.Position);

        var newFlatPosition = this.ApplyPosition(this.Position, flatPosition, distance);
        data.Position = new Vector3(newFlatPosition.X, data.Position.Y, newFlatPosition.Y);

        data.Tint = this.ApplyTint(this.Tint, data.Tint, distance);

        data.Rotation = this.ApplyRotation(this.Rotation, data.Rotation, distance);

        data.Scale = this.ApplyScale(this.Scale, data.Scale, distance);
    }

    public delegate T Mode<T>(T clump, T blade, float distance);
}

