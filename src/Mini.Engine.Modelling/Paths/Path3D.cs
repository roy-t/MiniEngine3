using System.Diagnostics;
using System.Numerics;

namespace Mini.Engine.Modelling.Paths;
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

    public Vector3[] ToArray(Vector3 offset)
    {
        var length = this.IsClosed ? this.Length + 1 : this.Length;
        var array = new Vector3[length];
        for (var i = 0; i < array.Length; i++)
        {
            array[i] = this[i] + offset;
        }

        return array;
    }

    public Vector3 GetForward(int index)
    {
        this.AssetValidPath();
        this.AssertValidIndex(index);

        if (this.IsClosed || index + 1 < this.Length)
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

    public Vector3 GetPositionAfterDistance(float distance)
    {
        (var index, var remainder) = this.GetIndexAfterDistance(distance);
        return this[index] + this.GetForward(index) * remainder;
    }

    public Vector3 GetForwardAfterDistance(float distance)
    {
        Debug.Assert(distance >= 0);

        (var index, var _) = this.GetIndexAfterDistance(distance);
        return this.GetForward(index);
    }

    public (int index, float remainder) GetIndexAfterDistance(float distance)
    {
        this.AssetValidPath();
        Debug.Assert(distance >= 0);

        var index = -1;
        var accumulator = 0.0f;
        var sectionDistance = 0.0f;

        do
        {
            accumulator += sectionDistance;
            index++;

            var from = this[index];
            var to = this[index + 1];
            sectionDistance = Vector3.Distance(from, to);
        } while (distance > accumulator + sectionDistance);

        return (index, distance - accumulator);
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
