﻿using System.Numerics;
using LibGame.Mathematics;
using LibGame.Physics;
using Mini.Engine.Core;

namespace Mini.Engine.Modelling.Curves;
public static class CurveExtensions
{
    public const float CurveStart = 0.0f;
    public const float CurveEnd = 1.0f;

    public static Vector3 GetLeft(this ICurve curve, float u)
    {
        var forward = curve.GetForward(u);
        return Vector3.Normalize(Vector3.Cross(Vector3.UnitY, forward));
    }

    public static ICurve OffsetLeft(this ICurve curve, float offset)
    {
        return new OffsetCurve(curve, offset);
    }

    public static ICurve OffsetRight(this ICurve curve, float offset)
    {
        return new OffsetCurve(curve, -offset);
    }

    public static ICurve Range(this ICurve curve, float start, float length)
    {
        return new RangeCurve(curve, start, length);
    }

    public static ICurve Reverse(this ICurve curve)
    {
        if (curve is ReverseCurve reverse)
        {
            return reverse.Curve;
        }

        return new ReverseCurve(curve);
    }

    public static float ComputeLengthPiecewise(this ICurve curve, int pieces = 1000)
    {
        var distance = 0.0f;
        var step = 1.0f / (pieces - 1.0f);
        for (var i = 0; i < (pieces - 1); i++)
        {

            var a = curve.GetPosition(step * (i + 0));
            var b = curve.GetPosition(step * (i + 1));

            distance += Vector3.Distance(a, b);
        }

        return distance;
    }

    public static ReadOnlySpan<Vector3> GetPoints(this ICurve curve, int points, Vector3 offset)
    {
        var vertices = new ArrayBuilder<Vector3>(points);
        var enumerator = new CurveIterator(curve, points);
        foreach (var position in enumerator)
        {
            vertices.Add(new Vector3(position.X, 0, -position.Y) + offset);
        }

        return vertices.Build();
    }

    /// <summary>
    /// Creates a transformation matrix that translates an object so that it is at `u` 
    /// and rotates the object so that it points in the same direction as the forward at `u`
    /// given the transformation first applied to the curve
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="u">The point on the curve</param>
    /// <param name="up">A vector that points towards what is relatively up for this curve. Prevents unwanted rolls in the transformation matrix</param>
    /// <param name="transform">The transformation to first apply to the curve</param>
    public static Matrix4x4 AlignTo(this ICurve curve, float u, Vector3 up, in Transform transform)
    {
        var position = curve.GetPosition(u);
        var forward = curve.GetForward(u);

        var matrix = new Transform(position, Quaternion.Identity, Vector3.Zero, 1.0f)
            .FaceTargetConstrained(position + forward, up)
            .GetMatrix();

        return matrix * transform.GetMatrix();
    }

    /// <summary>
    /// Creates a transformation matrix that translates an object so that it is at `u` 
    /// and rotates the object so that it points in the same direction as the forward at `u`
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="u">The point on the curve</param>
    /// <param name="up">A vector that points towards what is relatively up for this curve. Prevents unwanted rolls in the transformation matrix</param>    
    public static Matrix4x4 AlignTo(this ICurve curve, float u, Vector3 up)
    {
        return AlignTo(curve, u, up, Transform.Identity);
    }

    public static (Vector3 Position, Vector3 Forward) GetWorldOrientation(this ICurve curve, float u, in Transform transform)
    {
        var matrix = transform.GetMatrix();
        return GetWorldOrientation(curve, u, in matrix);
    }

    public static (Vector3 Position, Vector3 Forward) GetWorldOrientation(this ICurve curve, float u, in Matrix4x4 world)
    {
        var position = Vector3.Transform(curve.GetPosition(u), world);
        var forward = Vector3.TransformNormal(curve.GetForward(u), world);

        return (position, forward);
    }


    /// <summary>
    /// Checks if two curves are connected at the given parameterized positions and transformations.
    /// Two curves are connected if the squared distance between them is less `maxDistanceSquared`
    /// and the dot product between the two forward vectors is either close to zero or close to one
    /// </summary>
    /// <param name="curveA">The first curve</param>
    /// <param name="ua">Parameterized position on curve A</param>
    /// <param name="transformA">Transformation of curve A</param>
    /// <param name="curveB">The second curve</param>
    /// <param name="ub">Parameterized position on curve B</param>
    /// <param name="transformB">Transformation of curve B</param>
    /// <param name="maxDistanceSquared">The maximum squared distance between twe two position vectors for the two curves to be considered connected.</param>
    /// <param name="minCoherence">The minimum absolute value of the dot product between the two forward vectors for the two curves to be considered connected.</param>
    /// <returns></returns>
    public static bool IsConnectedTo(this ICurve curveA, float ua, in Transform transformA, ICurve curveB, float ub, in Transform transformB, float maxDistanceSquared, float minCoherence = 0.95f)
    {
        // Two curves are connected if the positions on their respective curves are close and their forwards either
        // point in exactly the same direction or in exactly the opposite direction
        var (positionA, forwardA) = curveA.GetWorldOrientation(ua, in transformA);
        var (positionB, forwardB) = curveB.GetWorldOrientation(ub, in transformB);

        if (Vector3.DistanceSquared(positionA, positionB) < maxDistanceSquared)
        {
            return Math.Abs(Vector3.Dot(forwardA, forwardB)) >= minCoherence;
        }

        return false;
    }

    /// <summary>
    /// Creates a transform that aligns the curve at position `u` with the given position and forward.
    /// Assumes the forward is in the XZ plane.
    /// </summary>
    /// <param name="curve">The curve</param>
    /// <param name="u">Parameterized position on the curve</param>
    /// <param name="position">The position to translate `u` to.</param>
    /// <param name="forward">The forward to align the curve to at position `u`.</param>
    public static Transform PlaceInXZPlane(this ICurve curve, float u, Vector3 position, Vector3 forward)
    {
        var yaw = Radians.YawFromVector(forward);
        var nextYaw = Radians.YawFromVector(curve.GetForward(u));
        var difference = Radians.DistanceRadians(nextYaw, yaw);

        var rotation = Quaternion.CreateFromYawPitchRoll(difference, 0.0f, 0.0f);
        var origin = curve.GetPosition(u);
        var transform = new Transform(position, rotation, origin, 1.0f);

        return transform;
    }

    public static bool TravelAlongCurve(this ICurve curve, float u, float distance, out float uNext)
    {
        var length = curve.Length;
        var portion = distance / length;

        uNext = u + portion;
        if (uNext > 1.0f)
        {
            uNext = 1.0f;
            return false;
        }
        
        return true;
    }

    public static bool TravelEucledianDistance(this ICurve curve, float u, float distance, float margin, out float uEnd)
    {
        // Traveling along a curve will get us at most to a position that is exactly `distance` away from our starting position.
        // But most likely it will bring less far. So we iteratively try to cover the remaining distance by traveling along the
        // curve again, without having to worry about overshooting it.

        var start = curve.GetPosition(u);
        var covered = 0.0f;

        do
        {            
            if (curve.TravelAlongCurve(u, distance - covered, out var uInter))
            {
                u = uInter;
                var end = curve.GetPosition(u);
                covered = Vector3.Distance(start, end);
            }
            else
            {
                uEnd = u;
                return false;
            }

        } while (covered < distance && Math.Abs(covered - distance) > margin);

        uEnd = u;
        return true;
    }
}
