using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.UI;

namespace Mini.Engine.Titan;

[Service]
internal class TitanMainMenuLoop : IGameLoop
{
    private readonly Device Device;
    private readonly UICore UserInterface;
    private readonly LoadingGameLoop LoadingScreen;

    public TitanMainMenuLoop(Device device, UICore ui, LoadingGameLoop loadingScreen)
    {
        this.Device = device;
        this.UserInterface = ui;
        this.LoadingScreen = loadingScreen;
    }

    public void Simulate()
    {

    }

    public void HandleInput(float elapsedRealWorldTime)
    {

    }

    public void Frame(float alpha, float elapsedRealWorldTime)
    {
        if (ImGui.Begin(nameof(TitanMainMenuLoop)))
        {
            if (ImGui.Button("Single Player"))
            {
                this.LoadingScreen.ReplaceCurrentGameLoop<TitanGameLoop>();
            }

            ImGui.End();
        }
    }

    public void Resize(int width, int height)
    {

    }

    public void Dispose()
    {

    }
}
