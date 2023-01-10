using System.Diagnostics;

namespace Mini.Engine.Core;

public enum Tristate
{
    Zero,
    One,
    Two
};

public static class BitPacker
{
    public static uint Pack(params Tristate[] elements)
    {
        Debug.Assert(elements.Length <= 16);
        
        var packed = 0u;

        for (var i = 0; i < elements.Length; i++)
        {
            var input = (uint)elements[i];
            var shifted = input << (i * 2);
            packed += shifted;
        }

        return packed;


    }

    public static Tristate[] Unpack(uint packed)
    {
        var output = new Tristate[8];

        for (var i = 0; i < output.Length; i++)
        {
            var mask = 0b0000_0000_0000_0011u << (i * 2);
            var shifted = packed & mask;
            var answer = shifted >> (i * 2);

            output[i] = (Tristate)answer;
        }

        return output;
    }
}
