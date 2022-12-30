using Mini.Engine.Core;
using Xunit;
namespace Mini.Engine.Tests;
public class ScratchPad
{

    [Fact]
    public void Foo()
    {
        var input = new uint[6]{ 0, 1, 2, 1, 2, 0 };
        var packed = BitPacker.Pack((Tristate)input[0], (Tristate)input[1], (Tristate)input[2], (Tristate)input[3], (Tristate)input[4], (Tristate)input[5]);
        var output = BitPacker.Unpack(packed).Select(t => (uint)t).Take(6).ToArray();


        Assert.Equal(input, output);
    }
}
