using System.Numerics;
using Mini.Engine.Graphics;
using Xunit;
using static Mini.Engine.Tests.FloatAssert;
using static Xunit.Assert;

namespace Mini.Engine.Tests;
public class TransformTests
{
    [Fact]
    public void SmokeTest()
    {
        StructTransform t = default;

        // Sane defaults

        Equal(Quaternion.Identity, t.GetRotation());        
        Equal(Vector3.Zero, t.GetPosition());
        Equal(-Vector3.UnitZ, t.GetForward());
        Equal(-Vector3.UnitX, t.GetLeft());
        Equal(Vector3.UnitY, t.GetUp());
        Equal(1.0f, t.GetScale());

        // Local and Global rotations

        t = default;

        t = t.AddLocalRotation(Quaternion.CreateFromYawPitchRoll(0, MathF.PI / 2, 0));

        AlmostEqual(new Vector3(0, 1, 0), t.GetForward());

        t = t.AddLocalRotation(Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0));

        AlmostEqual(new Vector3(-1, 0, 0), t.GetForward());

        t = t.AddRotation(Quaternion.CreateFromYawPitchRoll(0, 0, MathF.PI / 2));

        AlmostEqual(new Vector3(0, -1, 0), t.GetForward());

        t = t.SetRotation(Quaternion.Identity);
        AlmostEqual(new Vector3(0, 0, -1), t.GetForward());

        // Translations

        t = default;

        t = t.SetTranslation(Vector3.UnitY);
        AlmostEqual(Vector3.UnitY, t.GetPosition());

        t = t.AddTranslation(Vector3.UnitY);
        AlmostEqual(Vector3.UnitY * 2, t.GetPosition());

        // Combinding rotations and local translations

        t = default;

        t = t.SetRotation(Quaternion.CreateFromYawPitchRoll(MathF.PI / 2, 0, 0))
             .AddLocalTranslation(-Vector3.UnitZ);
        AlmostEqual(-Vector3.UnitX, t.GetPosition());

        // Scaling

        t = default;

        t = t.AddScale(0.5f).AddScale(0.5f);
        Equal(0.25f, t.GetScale());

        t = t.SetScale(0.65f);
        Equal(0.65f, t.GetScale());

        // Utility methods

        t = default;
        t = t.FaceTarget(Vector3.UnitX);
        AlmostEqual(Vector3.UnitX, t.GetForward());

        t = default;
        t = t.FaceTargetConstrained(Vector3.UnitX, Vector3.UnitY);
        AlmostEqual(Vector3.UnitX, t.GetForward());
        AlmostEqual(Vector3.UnitY, t.GetUp());

    }
}
