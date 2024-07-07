using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.UI;

namespace Mini.Engine.Titan;

[Service]
internal class TitanMainMenuLoop : IGameLoop
{
    private readonly Device Device;
    private readonly UICore UserInterface;

    public TitanMainMenuLoop(Device device, UICore ui)
    {
        this.Device = device;
        this.UserInterface = ui;
    }

    public void Simulate()
    {

    }

    public void HandleInput(float elapsedRealWorldTime)
    {

    }

    public void Frame(float alpha, float elapsedRealWorldTime)
    {
    }

    public void Resize(int width, int height)
    {
        this.UserInterface.Resize(width, height);
    }


    public void Dispose()
    {

    }
}
