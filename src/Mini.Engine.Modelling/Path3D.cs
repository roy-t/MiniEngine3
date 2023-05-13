using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Modelling;
public record struct Path3D(bool IsClosed, params Vector3[] Positions)
{
    public Vector3 this[int index]
    {
        get
        {
            this.AssertValidIndex(index);
            var i = Math.Abs(index) % this.Length;
            if (index < 0)
            {
                return this.Positions[^i];
            }
            else
            {
                return this.Positions[i];
            }
        }
        set
        {
            this.AssertValidIndex(index);
            var i = Math.Abs(index) % this.Length;
            if (index < 0)
            {
                this.Positions[^i] = value;
            }
            else
            {
                this.Positions[i] = value;
            }
        }
    }

    public int Length => this.Positions.Length;
    public int Steps => this.IsClosed ? this.Length : this.Length - 1;

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


    private Vector3 GetPositionAfterDistance(float distance)
    {
        Debug.Assert(distance >= 0);

       // TODO maybe get helper that gives you index and remainer?
       // what about closed vs not closed, fixed by index guard?

        // TODO: copy to path2D
    }

    private Vector3 GetNormalAfterDistance(float distance)
    {
        Debug.Assert(distance >= 0);

        // TODO maybe get helper that gives you index and remainer?
        // what about closed vs not closed, fixed by index guard?

        // TODO: copy to path2D
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
