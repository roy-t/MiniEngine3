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

    public static Format SelectHDRFormat(Mode mode, int components)
    {
        if (mode == Mode.Linear || mode == Mode.Normalized)
        {
            if (components == 2)
            {
                return Format.R32G32_Float;
            }

            //if (components == 3)
            //{
            //    return Format.R32G32B32_Float;
            //}

            if (components == 4)
            {
                return Format.R32G32B32A32_Float;
            }
        }

        throw new NotSupportedException();
    }

    public static Format SelectCompressedFormat(Mode mode, TranscodeFormats sourceFormat)
    {
        if (mode == Mode.Linear || mode == Mode.Normalized)
        {
            if (sourceFormat == TranscodeFormats.BC7_RGBA)
            {
                return Format.BC7_UNorm;
            }
        }
        else
        {
            if (sourceFormat == TranscodeFormats.BC7_RGBA)
            {
                return Format.BC7_UNorm_SRgb;
            }

            if (sourceFormat == TranscodeFormats.RGBA32)
            {
                return Format.B8G8R8A8_UNorm_SRgb;
            }
        }

        throw new NotSupportedException();
    }
}
