using System.Numerics;

namespace Mini.Engine.Core;
public sealed class ArrayBuilder<T>
    where T : struct
{
    private T[] array;

    public ArrayBuilder(int initialCapacity)
    {
        this.array = new T[initialCapacity];
        this.Length = 0;
    }

    public T this[int index]
    {
        get
        {
            if (index >= this.Length)
            {
                throw new IndexOutOfRangeException();
            }
            return this.array[index];
        }
        set
        {
            if (index >= this.Length)
            {
                throw new IndexOutOfRangeException();
            }
            this.array[index] = value;
        }
    }

    public int Length { get; private set; }
    public int Capacity => this.array.Length;

    public void Add(T item)
    {
        this.EnsureCapacity(this.Length + 1);
        this.array[this.Length++] = item;
    }

    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            this.Add(item);
        }
    }

    public void Add(ReadOnlySpan<T> span)
    {
        this.EnsureCapacity(this.Length + span.Length);
        span.CopyTo(this.array.AsSpan(this.Length, span.Length));
        this.Length += span.Length;
    }

    public void Clear()
    {
        this.Length = 0;
    }

    public int EnsureCapacity(int capacity)
    {
        if (capacity < this.array.Length)
        {
            return this.array.Length;
        }
        else
        {
            Array.Resize(ref this.array, capacity);
            return capacity;
        }
    }

    public ReadOnlySpan<T> Build()
    {
        return new ReadOnlySpan<T>(this.array, 0, this.Length);
    }
}
