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

    public Vector3 DepositionColor;
    public Vector3 ErosionColor;
    public float ErosionColorMultiplier;

    public HeightMapGeneratorSettings(int dimensions = 1024, Vector2 offset = default, float amplitude = 0.036f, float frequency = 1.653f, int octaves = 20, float lacunarity = 0.909f, float persistance = 0.600f, float cliffStart = 0.131f, float cliffEnd = 0.340f, float cliffStrength = 0.431f, float meshDefinition = 0.5f)
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

        this.DepositionColor = new Vector3(88.0f, 102.0f, 37.0f) / 255.0f;
        this.ErosionColor = new Vector3(178.0f, 160.0f, 112.0f) / 255.0f;
        this.ErosionColorMultiplier = 450.0f;
    }
}