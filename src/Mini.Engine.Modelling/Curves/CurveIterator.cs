using System.Collections;
using System.Numerics;

namespace Mini.Engine.Modelling.Curves;

public sealed class CurveIterator : IEnumerable<Vector3>, IEnumerator<Vector3>
{
    private readonly ICurve Curve;
    private readonly int Points;
    private readonly float Step;
    private int index;
    
    public CurveIterator(ICurve curve, int points)
    {
        this.Curve = curve;
        this.Points = points;
        this.Step = 1.0f / (this.Points - 1);

        this.index = -1;        
    }

    public Vector3 Current => this.Curve.GetPosition(this.index * this.Step);
        
    object IEnumerator.Current => this.Current;    
    
    public bool MoveNext()
    {        
        this.index++;
        return this.index < this.Points;
    }

    public void Reset()
    {
        this.index = -1;
    }

    public IEnumerator<Vector3> GetEnumerator()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this;
    }

    public void Dispose() { }
}
