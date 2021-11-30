using System;

namespace Mini.Engine.Content.Models.Wavefront;

internal ref struct SpanTokenEnumerator
{
    private readonly ReadOnlySpan<char> Source;
    private int head;
    private int tail;

    public SpanTokenEnumerator(ReadOnlySpan<char> source)
    {
        this.Source = source.Trim();
        this.Current = this.Source;
        this.head = 0;
        this.tail = 0;
    }

    public ReadOnlySpan<char> Current { get; private set; }

    public SpanTokenEnumerator GetEnumerator()
    {
        return this;
    }

    public bool MoveNext()
    {
        while (this.head < this.Source.Length)
        {
            var tailIsWhiteSpace = char.IsWhiteSpace(this.Source[this.tail]);
            var headIsWhiteSpace = char.IsWhiteSpace(this.Source[this.head]);

            if (!tailIsWhiteSpace && headIsWhiteSpace)
            {
                this.Current = this.Source.Slice(this.tail, this.head - this.tail);
                this.tail = this.head;
                return true;
            }

            if (tailIsWhiteSpace && !headIsWhiteSpace)
            {
                this.tail = this.head;
            }

            this.head++;
        }

        if (this.tail != this.head)
        {
            this.Current = this.Source.Slice(this.tail, this.head - this.tail);
            this.tail = this.head;
            return true;
        }

        return false;
    }

}
