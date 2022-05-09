namespace Mini.Engine.Graphics.World;

public sealed class HydraulicErosionBrushSettings
{
    /// <summary>
    /// Seed for randomizer
    /// </summary>
    public int Seed;

    /// <summary>
    /// Number of simulated droplets
    /// </summary>
    public int Droplets;

    /// <summary>
    /// Size of an individual droplet
    /// </summary>
    public int DropletStride;

    /// <summary>
    /// Multiplier for the amount of sediment one droplet of water can carry. Lower numbers produce a softer effect. Higher
    // numbers produce a stronger effect.
    // Range: [0.01f..5.0f]
    /// </summary>
    public float SedimentFactor;

    /// <summary>
    /// Sediment capacity of slow moving or standing still water. Lower numbers prevent cratering but might stop a droplet
    /// from affecting the terrain before the end of its lifetime. Higher numbers sometimes lead to craters and hills forming
    /// on flat surfaces.
    /// Range: [0..0.01]
    /// </summary>
    public float MinSedimentCapacity;

    /// <summary>
    /// Scales the speed of deposition when the droplet is going to slow to have enough capacity for all the sediment it
    /// has acquired. Lower numbers might allows the sediment to travel further and release slower. Higher numbers might lead
    /// to abrupt depositions, leading to spikey terrain.
    /// Range: [0.001f..1f]
    /// </summary>
    public float DepositSpeed;

    /// <summary>
    /// Inertia. 
    /// Controls how much water keeps going the same direction. Lower numbers make the water follow the contours of the 
    /// terrain better. Higher numbers allow the water to maintain its momentum and even allow it to flow slightly up
    /// Range: [0..1]
    /// </summary>
    public float Inertia;

    /// <summary>
    ///Affects the acceleration over time of water that is going up or down hill. Lower numbers reduce the effect on steep terrain.
    // Higher numbers increase the effect on steep terrain.
    // Range [1.0f, 20.0f]
    /// </summary>
    public float Gravity;

    public HydraulicErosionBrushSettings(int seed = 404, int droplets = 1_000_000, int dropletStride = 5, float sedimentFactor = 1.0f, float minSedimentCapacity = 0.000f, float depositSpeed = 0.01f, float inertia = 0.55f, float gravity = 4.0f)
    {
        this.Seed = seed;
        this.Droplets = droplets;
        this.DropletStride = dropletStride;
        this.SedimentFactor = sedimentFactor;
        this.MinSedimentCapacity= minSedimentCapacity;
        this.DepositSpeed = depositSpeed;
        this.Inertia = inertia;
        this.Gravity = gravity;
    }
}
