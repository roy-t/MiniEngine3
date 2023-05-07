namespace Mini.Engine.Core;
public static class ArrayUtilities
{
    public static T[] Concat<T>(params T[][] arrays)
    {
        var length = arrays.Sum(a => a.Length);
        var pool = new T[length];

        var counter = 0;
        for (var i = 0; i < arrays.Length; i++)
        {
            var array = arrays[i];
            Array.Copy(array, 0, pool, counter, array.Length);
            counter += array.Length;
        }

        return pool;
    }
}
