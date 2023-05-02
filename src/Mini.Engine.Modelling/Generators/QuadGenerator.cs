using System.Numerics;

namespace Mini.Engine.Modelling.Generators;
public static class QuadGenerator
{
    public static Quad Generate(Vector3 position, float extents)
    {
        var positions = new Vector3[]
        {
            position + new Vector3(extents, 0, -extents), // NE
            position + new Vector3(extents, 0, extents), // SE
            position + new Vector3(-extents, 0, extents), // SW
            position + new Vector3(-extents, 0, -extents), // NW
        };

        return new Quad(Vector3.UnitY, positions[0], positions[1], positions[2], positions[3]);
    }
}
