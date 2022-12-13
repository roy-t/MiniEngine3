using System.Diagnostics;

namespace Mini.Engine.Core;

// See
// - https://github.com/liuzl/alias/blob/master/alias.go
// - https://www.keithschwarz.com/darts-dice-coins/

public sealed class VoseWeightedRandom
{
    private struct FPiece
    {
        public float Prob;
        public uint Alias;
    }

    private struct IPiece
    {
        public uint Prob;
        public uint Alias;
    }

    private readonly IPiece[] Table;
    private readonly Random Random;

    public VoseWeightedRandom(Random random, IList<float> prob)
    {
        Debug.Assert(prob.Count > 0);

        var n = prob.Count;
        var total = 0.0f;

        for (var i = 0; i < n; i++)
        {
            Debug.Assert(prob[i] >= 0.0f);
            total += prob[i];
        }

        var table = new IPiece[n];
        var twins = new FPiece[n];

        var smallTop = -1;
        var largeBot = n;

        var mult = n / total;

        for (var i = 0; i < n; i++)
        {
            var p = prob[i];
            p *= mult;

            if (p >= 1)
            {
                largeBot--;
                twins[largeBot] = new FPiece { Prob = p, Alias = (uint)i };
            }
            else
            {
                smallTop++;
                twins[smallTop] = new FPiece { Prob = p, Alias = (uint)i };
            }
        }

        while (smallTop >= 0 && largeBot < n)
        {
            var l = twins[smallTop];
            smallTop--;

            var g = twins[largeBot];
            largeBot++;

            table[l.Alias].Prob = (uint)(l.Prob * int.MaxValue);
            table[l.Alias].Alias = g.Alias;

            g.Prob = (g.Prob + l.Prob) - 1;

            if (g.Prob < 1)
            {
                smallTop++;
                twins[smallTop] = g;
            }
            else
            {
                largeBot--;
                twins[largeBot] = g;
            }
        }

        for (var i = n - 1; i >= largeBot; i--)
        {
            table[twins[i].Alias].Prob = int.MaxValue;
        }
        for (var i = 0; i < smallTop; i++)
        {
            table[twins[i].Alias].Prob = int.MaxValue;
        }

        this.Table = table;
        this.Random = random;
    }

    public T Pick<T>(IReadOnlyList<T> items)
    {
        var index = this.Next();
        return items[index % items.Count];
    }

    public int Next()
    {
        var ri = this.Random.Next();
        var w = ri % this.Table.Length;

        if (ri > this.Table[w].Prob)
        {
            return (int)this.Table[w].Alias;
        }

        return w;
    }
}
