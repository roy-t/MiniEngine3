using System.Net;
using System.Net.Sockets;
using LiteNetLib;

namespace Mini.Engine.Titan.Multiplayer;
public sealed class LobbyHostState : IMultiplayerState
{
    private readonly Dictionary<int, int> LatencyList;

    public LobbyHostState()
    {
        this.LobbyPassword = string.Empty;

        this.LatencyList = new Dictionary<int, int>();
    }

    public IReadOnlyDictionary<int, int> Latency => this.LatencyList;

    public string LobbyPassword { get; set; }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        if (string.IsNullOrEmpty(this.LobbyPassword))
        {
            request.Accept();
        }
        else
        {
            request.AcceptIfKey(this.LobbyPassword);
        }
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        this.LatencyList[peer.Id] = latency;
    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {

    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {

    }

    public void OnPeerConnected(NetPeer peer)
    {

    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {

    }
}
