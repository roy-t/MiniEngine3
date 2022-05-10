using System.Numerics;

namespace Mini.Engine.Graphics.World;

public sealed class HeightMapGeneratorSettings
{

    /// <summary>
    /// Length of each dimension of the heightmap
    /// </summary>
    public int Dimensions;

    /// <summary>
    /// Offset applied to each coordinate in the heightmap, use this to find different parts of the terrain in the noise
    /// </summary>
    public Vector2 Offset;

    /// <summary>
    /// Amplitude of the first octave
    /// </summary>
    public float Amplitude;

    /// <summary>
    /// Frequency of the first octave
    /// </summary>
    public float Frequency;

    /// <summary>
    /// Number of octaves, or layers of noise
    /// </summary>
    public int Octaves;

    /// <summary>
    /// Increase in frequency for each consecutive octave, l * f ^ 0, l * f ^ 1, ...
    /// Range: (1..inf)
    /// </summary>
    public float Lacunarity;

    /// <summary>
    /// Decrease of amplitude for each consecutive octave, p * f ^ 0, p * f ^ 1, ...
    /// Range: (0..1) 
    /// </summary>
    public float Persistance;

    /// <summary>
    /// Start of more vertical cliffs
    /// </summary>
    public float CliffStart;

    /// <summary>
    /// End of more vertical cliffs
    /// </summary>
    public float CliffEnd;

    /// <summary>
    /// Strength of cliff effect
    /// </summary>
    public float CliffStrength;

    /// <summary>
    /// How many vertices are used to cover the height map
    /// Range: (0..1]
    /// </summary>
    public float MeshDefinition;

    public HeightMapGeneratorSettings(int dimensions = 512, Vector2 offset = default, float amplitude = 0.15f, float frequency = 1.5f, int octaves = 10, float lacunarity = 1.0f, float persistance = 0.55f, float cliffStart = 0.5f, float cliffEnd = 1.0f, float cliffStrength = 0.55f, float meshDefinition = 0.5f)
    {
        this.Dimensions = dimensions;
        this.Offset = offset;
        this.Amplitude = amplitude;
        this.Frequency = frequency;
        this.Octaves = octaves;
        this.Lacunarity = lacunarity;
        this.Persistance = persistance;
        this.CliffStart = cliffStart;
        this.CliffEnd = cliffEnd;
        this.CliffStrength = cliffStrength;
        this.MeshDefinition = meshDefinition;
    }
}