using Mini.Engine.Configuration;

namespace Mini.Engine.Titan.Multiplayer;

[Service]
public sealed class Session
{
    private readonly List<Player> PlayerList;

    public Session()
    {
        this.PlayerList = new List<Player>();
    }

    public IReadOnlyList<Player> Players => this.PlayerList;
    public Player? Host { get; private set; }

    public void SetHost(Player host)
    {
        if (!this.PlayerList.Contains(host))
        {
            throw new Exception("Host not present in session");
        }
        this.Host = host;
    }

    public void ClearHost()
    {
        this.Host = null;
    }

    public void AddPlayer(Player player)
    {
        this.PlayerList.Add(player);
    }

    public void RemovePlayer(Player player)
    {
        this.PlayerList.Remove(player);
    }
}
