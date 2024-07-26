using System.Net;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.DirectX;
using Mini.Engine.Titan.Multiplayer;
using Mini.Engine.UI;

namespace Mini.Engine.Titan;

[Service]
internal class TitanMainMenuLoop : IGameLoop
{


    private readonly Device Device;
    private readonly UICore UserInterface;
    private readonly LoadingGameLoop LoadingScreen;

    private string endPointString;
    private IPEndPoint? endPoint;
    private IPAddress? ipAddress;
    private short port;

    public TitanMainMenuLoop(Device device, UICore ui, LoadingGameLoop loadingScreen)
    {
        this.Device = device;
        this.UserInterface = ui;
        this.LoadingScreen = loadingScreen;

        this.endPointString = string.Empty;
        this.endPoint = null;
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

            if (ImGui.Button("Host Multiplayer"))
            {

                this.LoadingScreen.ReplaceCurrentGameLoop<TitanHostGameLoop>();
                // TODO: multiplayer
                // - Start hosting, show server screen
                // - Launch second instance, with connection string
                // - Flag ready
                // - Handle connect, start both instances, start gameloop
                // - PROFIT
            }

            ImGui.InputTextWithHint("IP Address", "127.0.0.1:" + MultiplayerConstants.DefaultPort, ref this.endPointString, (uint)MultiplayerConstants.Ipv4AddressMax.Length);
            ImGui.SameLine();

            this.endPoint = ParseHostAddress(this.endPointString);
            if (this.endPoint == null)
            {
                ImGui.BeginDisabled();
            }

            if (ImGui.Button("Join"))
            {

            }

            if (this.endPoint == null)
            {
                ImGui.EndDisabled();
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

    private static IPEndPoint? ParseHostAddress(string ipEndpoint)
    {
        if (string.IsNullOrWhiteSpace(ipEndpoint))
        {
            return null;
        }

        var parts = ipEndpoint.Split(':');
        if (parts.Length != 2)
        {
            return null;
        }

        if (!IPAddress.TryParse(parts[0], out var ipAddress))
        {
            return null;
        }


        if (!short.TryParse(parts[1], out var port))
        {
            return null;
        }

        if (port < 1024)
        {
            return null;
        }

        return new IPEndPoint(ipAddress, port);
    }

}
