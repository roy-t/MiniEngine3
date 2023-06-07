namespace Mini.Engine.ECS.Components;

// TODO: generify and move to LibGame?
public sealed class IndexTracker
{
    private static readonly Comparer<Record> OnEntityComparer = Comparer<Record>.Create((a, b) => a.Entity.CompareTo(b.Entity));

    private struct Record
    {
        public int Entity;
        public int Reference;
    }

    private readonly List<Record> Records;

    public IndexTracker(int initialCapacity)
    {
        this.Records = new List<Record>(initialCapacity);
    }

    public void InsertOrUpdate(Entity entity, int reference)
    {
        var record = new Record() { Entity = entity.Id, Reference = reference };
        var index = this.Records.BinarySearch(record, OnEntityComparer);

        if (index >= 0)
        {          
            this.Records[index] = record;
        }
        else
        {
            this.Records.Insert(~index, record);
        }        
    }

    public int Remove(Entity entity)
    {
        var record = new Record() { Entity = entity.Id, Reference = -1 };
        var index = this.Records.BinarySearch(record, OnEntityComparer);

        if (index < 0)
        {
            throw new Exception($"Could not find {entity}");
        }

        var reference = this.Records[index].Reference;
        this.Records.RemoveAt(index);

        return reference;
    }

    public int GetReference(Entity entity)
    {
        var record = new Record() { Entity = entity.Id, Reference = -1};
        var index = this.Records.BinarySearch(record, OnEntityComparer);

        if (index >= 0)
        {
            return this.Records[index].Reference;
        }

        throw new Exception($"Could not find {entity}");
    }

    public void Reserve(int capacity)
    {
        this.Records.Capacity = capacity;
    }
}
