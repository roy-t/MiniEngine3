using SuperCompressed;
using Vortice.DXGI;

namespace Mini.Engine.Content.Textures;
internal static class FormatSelector
{
    public static Format SelectSDRFormat(Mode mode, int components)
    {
        if (mode == Mode.Linear || mode == Mode.Normalized)
        {
            if (components == 1)
            {
                return Format.R8_UNorm;
            }
            else if (components == 2)
            {
                return Format.R8G8_UNorm;
            }
            else if (components == 4)
            {
                return Format.R8G8B8A8_UNorm;
            }
        }
        else
        {
            if (components == 4)
            {
                return Format.R8G8B8A8_UNorm_SRgb;
            }
        }

        throw new NotSupportedException();
    }

    // TODO: check wherever this should be used and add HDR component
}
