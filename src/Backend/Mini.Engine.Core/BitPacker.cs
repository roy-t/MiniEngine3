namespace Mini.Engine.Core;

public enum Tristate
{
    Zero,
    One,
    Two
};

public static class BitPacker
{
    public static uint Pack(Tristate a, Tristate b = Tristate.Zero, Tristate c = Tristate.Zero, Tristate d = Tristate.Zero, Tristate e = Tristate.Zero, Tristate f = Tristate.Zero, Tristate g = Tristate.Zero, Tristate h = Tristate.Zero)
    {
        var input = new uint[8]
        {
            (uint)a, (uint)b, (uint)c, (uint)d,
            (uint)e, (uint)f, (uint)g, (uint)h
        };

        var packed = 0u;

        for (var i = 0; i < input.Length; i++)
        {
            var shifted = input[i] << (i * 2);
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
