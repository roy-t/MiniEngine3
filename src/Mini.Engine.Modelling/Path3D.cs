using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Modelling;
public record struct Path3D(bool IsClosed, params Vector3[] Positions)
{
    public Vector3 this[int index]
    {
        get
        {
            var i = this.IsClosed ? (index % this.Positions.Length) : index;
            if (this.IsClosed && i < 0) { i += this.Length; }
            return this.Positions[i];
        }
        set
        {
            var i = this.IsClosed ? (index % this.Positions.Length) : index;
            if (this.IsClosed && i < 0) { i += this.Length; }
            this.Positions[i] = value;
        }
    }

    public int Length => this.Positions.Length;

    public Vector3 GetForward(int index)
    {
        this.AssetValidPath();
        this.AssertValidIndex(index);

        if (this.IsClosed || (index + 1) < this.Length)
        {
            var from = this[index];
            var to = this[index + 1];

            return Vector3.Normalize(to - from);
        }
        else
        {
            // If the path is not closed, the forward direction of the last position
            // is the same as the second to last one.
            var from = this[index - 1];
            var to = this[index];

            return Vector3.Normalize(to - from);
        }
    }

    public Vector3 GetBackward(int index)
    {
        this.AssetValidPath();
        this.AssertValidIndex(index);

        if (this.IsClosed || index > 0)
        {
            var from = this[index];
            var to = this[index - 1];

            return Vector3.Normalize(to - from);
        }
        else
        {
            // If the path is not closed, the backward direction of the first position
            // is the same as the second one.
            var from = this[index + 1];
            var to = this[index];

            return Vector3.Normalize(to - from);
        }
    }

    public Vector3 GetForwardAlongBendToNextPosition(int index)
    {
        this.AssetValidPath();
        this.AssertValidIndex(index);


        if (this.IsClosed || index > 0)
        {
            return Vector3.Normalize(this.GetForward(index - 1) + this.GetForward(index));
        }

        return this.GetForward(index);
    }

    [Conditional("DEBUG")]
    private void AssertValidIndex(int index)
    {
        if (!this.IsClosed)
        {
            Debug.Assert(index >= 0 && index < this.Length);
        }
    }

    [Conditional("DEBUG")]
    private void AssetValidPath()
    {
        Debug.Assert(this.Length >= 2);
    }
}
