﻿using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using LibGame.Mathematics;
using Mini.Engine.Core;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Diesel;
using Mini.Engine.Modelling.Curves;
using Mini.Engine.Modelling.Paths;

namespace Mini.Engine.Modelling.Tools;
public static class Extruder
{
    [Obsolete]
    public static Quad[] Extrude(Path2D crossSection, float depth, bool closeShape = true)
    {
        var layout = new Path3D(closeShape, Vector3.Zero, Vector3.UnitZ * -depth);
        return Extrude(crossSection, layout, Vector3.UnitY);
    }


    [Obsolete]
    public static Quad[] Extrude(Path2D crossSection, Path3D path)
    {
        return Extrude(crossSection, path, Vector3.UnitY);
    }

    [Obsolete]
    public static Quad[] Extrude(Path2D crossSection, Path3D path, Vector3 up)
    {
        if (crossSection.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        if (path.Length < 2)
        {
            throw new Exception("Invalid path");
        }

        var quads = new Quad[path.Steps * crossSection.Steps];

        var counter = 0;
        for (var i = 0; i < path.Steps; i++)
        {
            var positionA = path[i];
            var directionA = path.GetForwardAlongBendToNextPosition(i);
            var matrixA = new Transform(positionA, Quaternion.Identity, Vector3.Zero, 1.0f)
                .FaceTargetConstrained(positionA + directionA, up)
                .GetMatrix();

            var positionB = path[i + 1];
            var directionB = path.GetForwardAlongBendToNextPosition(i + 1);
            var matrixB = new Transform(positionB, Quaternion.Identity, Vector3.Zero, 1.0f)
                .FaceTargetConstrained(positionB + directionB, up)
                .GetMatrix();

            for (var j = 0; j < crossSection.Steps; j++)
            {
                var topRight = Vector3.Transform(crossSection[j + 1].WithZ(), matrixB);
                var bottomRight = Vector3.Transform(crossSection[j + 1].WithZ(), matrixA);
                var bottomLeft = Vector3.Transform(crossSection[j].WithZ(), matrixA);
                var topLeft = Vector3.Transform(crossSection[j].WithZ(), matrixB);

                quads[counter++] = new Quad(topRight, bottomRight, bottomLeft, topLeft);
            }
        }


        return quads;
    }

    [Obsolete]
    public static Quad[] Extrude(Path2D crossSection, ICurve curve, int points, Vector3 up)
    {
        if (crossSection.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        if (points < 2)
        {
            throw new Exception("Invalid points");
        }

        var steps = points - 1;

        var quads = new Quad[steps * crossSection.Steps];

        var counter = 0;
        for (var i = 0; i < steps; i++)
        {
            var uC = (i + 0) / (float)steps;
            var uN = (i + 1) / (float)steps;

            var positionA = curve.GetPosition3D(uC);
            var directionA = curve.GetNormal3D(uC);
            var matrixA = new Transform(positionA, Quaternion.Identity, Vector3.Zero, 1.0f)
                .FaceTargetConstrained(positionA + directionA, up)
                .GetMatrix();

            var positionB = curve.GetPosition3D(uN);
            var directionB = curve.GetNormal3D(uN);

            var matrixB = new Transform(positionB, Quaternion.Identity, Vector3.Zero, 1.0f)
                .FaceTargetConstrained(positionB + directionB, up)
                .GetMatrix();

            for (var j = 0; j < crossSection.Steps; j++)
            {
                var topRight = Vector3.Transform(crossSection[j + 1].WithZ(), matrixB);
                var bottomRight = Vector3.Transform(crossSection[j + 1].WithZ(), matrixA);
                var bottomLeft = Vector3.Transform(crossSection[j].WithZ(), matrixA);
                var topLeft = Vector3.Transform(crossSection[j].WithZ(), matrixB);

                quads[counter++] = new Quad(topRight, bottomRight, bottomLeft, topLeft);
            }
        }


        return quads;
    }

    public static void Extrude(IPrimitiveMeshPartBuilder builder, Path2D crossSection, ICurve curve, int points, Vector3 up)
    {
        if (crossSection.Length < 2)
        {
            throw new Exception("Invalid cross section");
        }

        if (points < 2)
        {
            throw new Exception("Invalid points");
        }

        var steps = points - 1;
        for (var i = 0; i < steps; i++)
        {
            var uC = (i + 0) / (float)steps;            
            var matrixA = curve.AlignTo(uC, up);

            var uN = (i + 1) / (float)steps;
            var matrixB = curve.AlignTo(uN, up);

            for (var j = 0; j < crossSection.Steps; j++)
            {
                var topRight = Vector3.Transform(crossSection[j + 1].WithZ(), matrixB);
                var bottomRight = Vector3.Transform(crossSection[j + 1].WithZ(), matrixA);
                var bottomLeft = Vector3.Transform(crossSection[j].WithZ(), matrixA);
                var topLeft = Vector3.Transform(crossSection[j].WithZ(), matrixB);

                builder.AddQuad(topRight, bottomRight, bottomLeft, topLeft);
            }
        }
    }
}
