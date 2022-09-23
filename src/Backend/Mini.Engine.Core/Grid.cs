namespace Mini.Engine.Core;
public sealed class Grid<T>
{
    private readonly T[] Items;

    public Grid(float minX, float maxX, float minY, float maxY, int columns, int rows)
    {
        if (minX >= maxX || minY >= maxY || columns < 1 || rows < 1)
        {
            throw new Exception("Illegal argument");
        }

        this.Epsilon = (maxX - minX) / 1000.0f;
        this.MinX = minX;
        this.MaxX = maxX;
        this.MinY = minY;
        this.MaxY = maxY;
        this.Columns = columns;
        this.Rows = rows;

        this.Items = new T[rows * columns];
    }
    public float Epsilon { get; }
    public float MinX { get; }
    public float MaxX { get; }
    public float MinY { get; }
    public float MaxY { get; }
    public int Columns { get; }
    public int Rows { get; }

    public void Fill(Func<float, float, int, int, T> generator)
    {
        for (var i = 0; i < this.Items.Length; i++)
        {
            var (ix, iy) = Indexes.ToTwoDimensional(i, this.Columns);
            var (xv, yv) = GetValue(ix, iy);

            this.Items[i] = generator(xv, yv, ix, iy);
        }
    }

    public void Put(T item, float x, float y)
    {
        var (ix, iy) = this.GetPosition(x, y);

        var index = Indexes.ToOneDimensional(ix, iy, this.Columns);
        this.Items[index] = item;
    }

    public T Get(float x, float y)
    {
        var (ix, iy) = this.GetPosition(x, y);

        var index = Indexes.ToOneDimensional(ix, iy, this.Columns);
        return this.Items[index];
    }

    private (int x, int y) GetPosition(float x, float y)
    {
        x = Math.Clamp(x, this.MinX, this.MaxX - this.Epsilon);
        y = Math.Clamp(y, this.MinY, this.MaxY - this.Epsilon);

        var xRange = this.MaxX - this.MinX;
        var xIndex = (int)(((x - this.MinX) / xRange) * this.Columns);

        var yRange = this.MaxY - this.MinY;
        var yIndex = (int)(((y - this.MinY) / yRange) * this.Rows);

        return (xIndex, yIndex);
    }

    private (float x, float y) GetValue(int x, int y)
    {
        x = Math.Clamp(x, 0, this.Columns - 1);
        y = Math.Clamp(y, 0, this.Rows - 1);

        var xRange = this.MaxX - this.MinX;
        var xValue = this.MinX + (x / (float)this.Columns) * xRange;
        xValue += (xRange / this.Columns) * 0.5f;

        var yRange = this.MaxY - this.MinY;
        var yValue = this.MinY + (y / (float)Rows) * yRange;
        yValue += (yRange / this.Rows) * 0.5f;

        return (xValue, yValue);
    }
}
