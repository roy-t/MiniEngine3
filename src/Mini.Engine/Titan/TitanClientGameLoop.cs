using System.Net;
using ImGuiNET;
using Mini.Engine.Configuration;
using Mini.Engine.Titan.Multiplayer;

namespace Mini.Engine.Titan;
[Service]
public sealed class TitanClientGameLoop : IGameLoop
{
    private readonly MultiplayerClient Client;

    public TitanClientGameLoop(MultiplayerClient client)
    {
        this.Client = client;
    }

    public void Enter()
    {
        // TODO: make it possible to set ip to connect to and the correct key
        // how do we pass data between screens?
        var endPoint = new IPEndPoint(IPAddress.Loopback, MultiplayerConstants.DefaultPort);
        this.Client.Connect(endPoint, MultiplayerConstants.ConnectionHandshakeKey);
    }


    public void Simulate()
    {

    }

    public void HandleInput(float elapsedRealWorldTime)
    {

    }

    public void Frame(float alpha, float elapsedRealWorldTime)
    {
        if (ImGui.Begin(nameof(TitanClientGameLoop)))
        {

        }
    }

    public void Dispose()
    {

    }
}
