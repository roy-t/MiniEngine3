using System.Collections.Generic;
using System.Numerics;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics.Models;

public sealed class Visibility
{
    private readonly List<Entity> Entities;

    public Visibility()
    {
        this.Entities = new List<Entity>();
    }

    public IReadOnlyList<Entity> Visible => this.Entities;

    public void Clear()
    {
        this.Entities.Clear();
    }

    public void Test(ModelComponent model, Matrix4x4 viewProjection)
    {
        
    }
}
