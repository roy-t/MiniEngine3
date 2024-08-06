namespace Mini.Engine.Titan.Multiplayer;
public static class MultiplayerConstants
{
    public static readonly string Ipv4AddressMax = "255.255.255.254:65536";
    public static readonly string Ipv4AddressMin = "1.0.0.0:1";
    public const int DefaultPort = 6577;
    public const int MaxPeers = 10;
    public static readonly string ConnectionHandshakeKey = "Mini.Engine.Handshake";
}
