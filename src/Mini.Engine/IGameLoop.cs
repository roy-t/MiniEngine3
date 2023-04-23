using System;

namespace Mini.Engine;

internal interface IGameLoop : IDisposable
{
    void Draw(float alpha);
    void Update(float elapsedSimulationTime, float elapsedRealWorldTime);
    void Resize(int width, int height);
}