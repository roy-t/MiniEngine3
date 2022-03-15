using System;

namespace Mini.Engine;

internal interface IGameLoop : IDisposable
{
    void Draw(float alpha);
    void Update(float time, float elapsed);
    void Resize(int width, int height);
}