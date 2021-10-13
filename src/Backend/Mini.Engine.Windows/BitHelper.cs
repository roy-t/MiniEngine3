using System;

namespace Mini.Engine.Windows
{
    public static class BitHelper
    {
        public static bool IsSet(IntPtr param, int bit)
        {
            if (IntPtr.Size == 8)
            {

                return ((ulong)param & (1ul << bit)) != 0;
            }

            return ((uint)param & (1u << bit)) != 0;
        }
    }
}
