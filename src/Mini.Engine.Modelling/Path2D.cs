using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Modelling;

public record struct Path2D(bool IsClosed, params Vector2[] Positions)
{
    public Vector2 this[int index]
    {
        get => this.Positions[index % this.Positions.Length];
        set => this.Positions[index % this.Positions.Length] = value;
    }

    public int Length => this.Positions.Length;

    public Vector2 GetForward(int index)
    {
        this.AssertValidIndex(index);

        if (index == 0 && this.Length > 1)
        {
            return Vector2.Normalize(this[1] - this[0]);
        }

        if (!this.IsClosed && index == this.Length - 1)
        {
            // Continue in the direction we were going
            return Vector2.Normalize(this[index] - this[index - 1]);
        }

        return Vector2.Normalize(this[index + 1] - this[index]);
    }

    public Vector2 GetBackward(int index)
    {
        this.AssertValidIndex(index);

        if (!this.IsClosed && index == 0 && this.Length > 1)
        {
            // Continue in the direction we were going
            return Vector2.Normalize(this[0] - this[1]);
        }

        if (index == this.Length - 1)
        {
            return Vector2.Normalize(this[index - 1] - this[index]);
        }

        return Vector2.Normalize(this[index - 1] - this[index]);
    }

    public Vector2 GetForwardAlongBendToNextPosition(int index)
    {
        this.AssertValidIndex(index);

        if (index == 0)
        {
            return this.GetForward(index);
        }

        return Vector2.Normalize(this.GetForward(index - 1) + this.GetForward(index));
    }

    [Conditional("DEBUG")]
    private void AssertValidIndex(int index)
    {
        Debug.Assert(index >= 0 && (this.IsClosed || index < this.Length));
    }
}
