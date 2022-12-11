using System.Numerics;
using Mini.Engine.Core;
using Mini.Engine.Graphics;
using Mini.Engine.Graphics.Cameras;
using Xunit;
namespace Mini.Engine.Tests;
public class ScratchPad
{

    [Fact]
    public void Foo()
    {
        var width = 1920.0f;
        var heigth = 1080.0f;
        var camera = new PerspectiveCamera(0.1f, 250.0f, MathF.PI / 2.0f, width / heigth);

        var sequence = new QuasiRandomSequence(6);
        var w = 2.0f * width;
        var h = 2.0f * heigth;

        var currentJitter = Vector2.Zero;
        var previousJitter = Vector2.Zero;
        for (var i = 0; i < 3; i++)
        {
            previousJitter = currentJitter;
            currentJitter = sequence.Next2D(-1.0f / w, 1.0f / w, -1.0f / h, 1.0f / h);
        }

        var transform = Transform.Identity;

        var one = Vector4.One;

        var current = Vector4.Transform(one, camera.GetInfiniteReversedZViewProjection(transform, currentJitter));
        var currentU = Vector4.Transform(one, camera.GetInfiniteReversedZViewProjection(transform, Vector2.Zero));
        current /= current.W;
        currentU /= currentU.W;

        var c2 = new Vector2(current.X, current.Y);
        var cU2 = new Vector2(currentU.X, currentU.Y);

        var previous = Vector4.Transform(one, camera.GetInfiniteReversedZViewProjection(transform, previousJitter));
        var previousU = Vector4.Transform(one, camera.GetInfiniteReversedZViewProjection(transform, Vector2.Zero));
        previous /= previous.W;
        previousU /= previousU.W;

        var p2 = new Vector2(previous.X, previous.Y);
        var pU2 = new Vector2(previousU.X, previousU.Y);


        var d = p2 - c2;
        var du = pU2 - cU2;
        var dx = (p2 - previousJitter) - (c2 - currentJitter);
        var dxx = (p2 - c2) -  (previousJitter - currentJitter);

        var dxy = ((p2 - c2) - currentJitter) - previousJitter;


        return;

    }
}
