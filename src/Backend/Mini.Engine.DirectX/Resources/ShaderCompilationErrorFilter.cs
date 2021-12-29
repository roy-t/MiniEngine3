using System;
using System.Text;
using Vortice.Direct3D;

namespace Mini.Engine.DirectX.Resources;

public static class ShaderCompilationErrorFilter
{
    public static void ThrowOnWarningOrError(Blob errorBlob, params string[] ignores)
    {
        if (errorBlob != null)
        {
            var output = Encoding.ASCII.GetString(errorBlob.GetBytes()).Substring(0, errorBlob.BufferSize - 1);
            var messages = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            var error = new StringBuilder();
            foreach (var message in messages)
            {
                if (!IsIgnored(message, ignores))
                {
                    error.AppendLine(message);
                }
            }

            if (error.Length > 0)
            {
                throw new Exception(error.ToString());
            }
        }
    }

    private static bool IsIgnored(string message, params string[] ignores)
    {
        foreach (var ignore in ignores)
        {
            if (message.Contains(ignore, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
