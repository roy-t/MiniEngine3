using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mini.Engine.Tests;
public class Memories
{
    record struct Quad(Vector3 Normal, Vector3 A, Vector3 B, Vector3 C, Vector3 D);

    [Fact]
    public void CanRentMemory()
    {
        using var pool = MemoryPool<Quad>.Shared.Rent();
 
        var memory = pool.Memory;
        Fill(memory, 10000);


        var whoot = memory.ToArray();

        Assert.Equal(10000, whoot.Length);
    }

    private static void Fill(Memory<Quad> memory, int count)
    {
        var span = memory.Span;
        for (var i = 0; i < count; i++)
        {
            span[i] = new Quad(Vector3.One, Vector3.One, Vector3.One, Vector3.One, Vector3.One);
        }
    }
}
