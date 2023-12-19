namespace Mini.Engine;

internal interface IGameLoop : IDisposable
{    
    void Update(float elapsedSimulationTime);
    void Draw(float alpha, float elapsedRealWorldTime);
    void Resize(int width, int height);
}