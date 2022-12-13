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
        var random = new VoseWeightedRandom(Random.Shared, new float[] { 0.25f, 0.50f, 0.25f });

        var outcomes = new int[3];
        var references = new int []{ 0, 1, 2 };
        for (var i = 0; i < 1_000_000; i++)
        {
            var index = random.Pick(references);
            outcomes[index]++;
        }


        if (outcomes[0] > 0)
        {

        }
    }
}
