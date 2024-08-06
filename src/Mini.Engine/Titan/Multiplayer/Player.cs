using LiteNetLib;

namespace Mini.Engine.Titan.Multiplayer;
public sealed class Player : IEquatable<Player?>
{
    public static Player Generate()
    {
        var id = Guid.NewGuid();
        var name = Environment.UserName;
        return new Player(id, name);
    }

    public Player(Guid id, string alias)
        : this(null, id, alias) { }

    public Player(NetPeer? peer, Guid id, string alias)
    {
        this.Peer = peer;
        this.Id = id;
        this.Alias = alias;
    }

    public NetPeer? Peer { get; }
    public Guid Id { get; }
    public string Alias { get; set; }

    public override bool Equals(object? obj)
    {
        return this.Equals(obj as Player);
    }

    public bool Equals(Player? other)
    {
        return other is not null &&
               this.Id.Equals(other.Id);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(this.Id);
    }

    public static bool operator ==(Player? left, Player? right) => EqualityComparer<Player>.Default.Equals(left, right);
    public static bool operator !=(Player? left, Player? right) => !(left == right);
}
