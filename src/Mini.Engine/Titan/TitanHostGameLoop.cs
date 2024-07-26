using System.Net;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Titan.Multiplayer;
using Mini.Engine.UI;

namespace Mini.Engine.Titan;

[Service]
internal class TitanHostGameLoop : IGameLoop
{
    private readonly UICore UserInterface;
    private readonly LoadingGameLoop LoadingScreen;
    private readonly Session Session;
    private readonly Player Host;
    private Player? Selected;

    public TitanHostGameLoop(UICore ui, LoadingGameLoop loadingScreen, Session session)
    {
        this.UserInterface = ui;
        this.LoadingScreen = loadingScreen;
        this.Session = session;

        this.Host = Player.Generate();
    }

    public void Enter()
    {
        this.Session.AddPlayer(this.Host);
        this.Session.SetHost(this.Host);
    }

    public void Simulate()
    {

    }

    public void HandleInput(float elapsedRealWorldTime)
    {

    }

    public void Frame(float alpha, float elapsedRealWorldTime)
    {
        if (ImGui.Begin(nameof(TitanHostGameLoop)))
        {
            if (ImGui.BeginListBox("Players"))
            {
                foreach (var player in this.Session.Players)
                {
                    var selected = this.Selected == player;
                    if (ImGui.Selectable($"{(player == this.Session.Host ? "* " : string.Empty)}{player.Alias} ({player.Id})", selected))
                    {
                        this.Selected = player;
                    }
                }

                ImGui.EndListBox();
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
