using System.Collections.Generic;
using System.Threading;
using Mini.Engine.Configuration;

namespace Mini.Engine.ECS.Entities;

[Service]
public sealed class EntityAdministrator
{
    // TODO: class assumes single threaded entity creation

    private readonly List<Entity> EntityList;
    private int nextId = 0;

    public EntityAdministrator()
    {
        this.EntityList = new List<Entity>();
    }

    public IReadOnlyList<Entity> Entities => this.EntityList;

    public Entity Create()
    {        
        var entity = new Entity(++this.nextId);
        this.EntityList.Add(entity);

        return entity;
    }
       
    public void Remove(Entity entity)
    {
        this.EntityList.Remove(entity);
    }    
}
