using System.Diagnostics;
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
    private readonly MultiplayerHost Host;
    private readonly LobbyHostState State;

#if DEBUG
    private bool withDebugger;
#endif

    public TitanHostGameLoop(UICore ui, LoadingGameLoop loadingScreen, MultiplayerHost host)
    {
        this.UserInterface = ui;
        this.LoadingScreen = loadingScreen;
        this.Host = host;
        this.State = new LobbyHostState();
    }

    public void Enter()
    {
        this.Host.Start(MultiplayerConstants.DefaultPort);
        this.Host.SwitchState(this.State);
    }

    public void Exit()
    {
        // TODO: only stop when we don't launch the session
        this.Host.Stop();
    }

    public void Simulate()
    {

    }

    public void HandleInput(float elapsedRealWorldTime)
    {

    }

    public void Frame(float alpha, float elapsedRealWorldTime)
    {
        this.Host.Update();

        if (ImGui.Begin(nameof(TitanHostGameLoop)))
        {
            if (ImGui.BeginListBox("Players"))
            {
                // TODO: figure out hosts public-ip and port, via Steam API
                ImGui.Selectable($"You, address: :{MultiplayerConstants.DefaultPort}, latency: 0 ms", false);

                foreach (var client in this.Host.ConnectedPeers)
                {
                    var latency = 999;
                    if (this.State.Latency.TryGetValue(client.Id, out var l))
                    {
                        latency = l;
                    }
                    ImGui.Selectable($"Id: {client.Id}, address: {client.Address}:{client.Port}, latency: {l} ms", false);
                }

                //foreach (var player in this.Session.Players)
                //{
                //    var selected = this.Selected == player;
                //    if (ImGui.Selectable($"{(player == this.Session.Host ? "* " : string.Empty)}{player.Alias} ({player.Id})", selected))
                //    {
                //        this.Selected = player;
                //    }
                //}

                ImGui.EndListBox();
            }

#if DEBUG
            if (ImGui.Button("Spawn client process"))
            {
                var cla = Environment.GetCommandLineArgs();
                var exe = cla[0];
                exe = exe.Replace(".dll", ".exe"); // when starting from Visual Studio we start in the dll
                var args = cla[1..];

                if (Environment.MachineName.Equals("Creature24", StringComparison.OrdinalIgnoreCase))
                {
                    // HACK: easy positioning on my machine
                    args = [.. args, "--position", "3841,16,1270,1415"];
                }

                if (this.withDebugger)
                {
                    args = [.. args, "--debugger"];
                }

                var info = new ProcessStartInfo(exe, args);
                Process.Start(info);
            }
            ImGui.Checkbox("With Debugger", ref this.withDebugger);
#endif

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
