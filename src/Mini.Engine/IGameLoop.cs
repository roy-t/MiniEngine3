namespace Mini.Engine;

public interface IGameLoop : IDisposable
{
    /// <summary>
    /// Perform one step in the simulation, independent of external input or framerate
    /// </summary>    
    void Simulate();

    /// <summary>
    /// Process input
    /// </summary>
    /// <param name="elapsedRealWorldTime">Elapsed real world time since the last call to Frame, useful for making input independent of frame rate</param>
    void HandleInput(float elapsedRealWorldTime);

    /// <summary>
    /// Calculate and render a new frame.
    /// </summary>
    /// <param name="alpha">Interpolation value between the last simulation step (0.0) and the next simulation step (1.0), can be used to smooth movement of any item that only moves during the simulation step</param>
    /// <param name="elapsedRealWorldTime">Elapsed real world time since the last call to Frame, useful for making input independent of frame rate</param>
    void Frame(float alpha, float elapsedRealWorldTime);

    /// <summary>
    /// Resize the screen
    /// </summary>    
    void Resize(int width, int height);
}