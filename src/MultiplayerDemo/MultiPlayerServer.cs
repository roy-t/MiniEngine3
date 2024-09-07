namespace MultiplayerDemo;
public sealed class MultiPlayerServer
{
    // Pretend there is a multiplayer server and that we can send receive message here from each other

    private readonly SemaphoreSlim Semaphore;
    private readonly Dictionary<int, List<Message>> Messages;
    private readonly List<int> ClientList;

    private volatile int NextClientId = 1;

    public MultiPlayerServer()
    {
        this.Semaphore = new SemaphoreSlim(1);
        this.Messages = new Dictionary<int, List<Message>>();
        this.ClientList = new List<int>();
    }

    public IReadOnlyList<int> Clients => this.ClientList;

    public void Host()
    {
        this.ClientList.Add(0);
    }

    public int Connect()
    {
        if (this.Clients.Count == 0)
        {
            throw new Exception("No Host");
        }

        var id = this.NextClientId++;
        this.ClientList.Add(id);
        return id;
    }

    public void Disconnect(int clientId)
    {
        this.ClientList.Remove(clientId);
    }

    public void BroadcastMessage(Message message)
    {
        if (message.SourceId != 0)
        {
            throw new Exception("Only the host can broadcast");
        }

        foreach (var client in this.ClientList)
        {
            if (client != 0)
            {
                this.SendMessage(message with { DestinationId = client });
            }
        }
    }

    public void SendMessage(Message message)
    {
        if (message.SourceId == 0 && message.DestinationId == 0)
        {
            throw new Exception("The host cannot send message to itself");
        }

        if (message.SourceId != 0 && message.DestinationId != 0)
        {
            throw new Exception("Clients can only send message to the host");
        }

        this.Semaphore.Wait();
        if (!this.Messages.TryGetValue(message.DestinationId, out var list))
        {
            list = new List<Message>();
            this.Messages[message.DestinationId] = list;
        }
        this.Messages[message.DestinationId].Add(message);
        this.Semaphore.Release();
    }

    public List<Message> ReceiveMessage(int clientId)
    {
        var messages = new List<Message>();

        this.Semaphore.Wait();

        if (this.Messages.TryGetValue(clientId, out var list))
        {
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (list[i].DestinationId == clientId)
                {
                    messages.Add(list[i]);
                    list.RemoveAt(i);
                }
            }
        }

        this.Semaphore.Release();


        return messages;
    }
}
