using Mini.Engine.Content.Serialization;

namespace Mini.Engine.Content.Shaders;
public static class SerializationExtensions
{
    public static void Write(this ContentWriter writer, ComputeShaderSettings settings)
    {
        writer.Write(settings.NumThreadsX);
        writer.Write(settings.NumThreadsY);
        writer.Write(settings.NumThreadsZ);
    }

    public static ComputeShaderSettings ReadComputeShaderSettings(this ContentReader reader)
    {
        var numThreadsX = reader.ReadInt();
        var numThreadsY = reader.ReadInt();
        var numThreadsZ = reader.ReadInt();

        return new ComputeShaderSettings(numThreadsX, numThreadsY, numThreadsZ);
    }
}
