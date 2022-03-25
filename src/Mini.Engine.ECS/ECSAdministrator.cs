using Mini.Engine.Configuration;
using Mini.Engine.ECS.Components;
using Mini.Engine.ECS.Entities;

namespace Mini.Engine.ECS;

[Service]
public sealed class ECSAdministrator
{
    public ECSAdministrator(EntityAdministrator entities, ComponentAdministrator components)
    {
        this.Entities = entities;
        this.Components = components;
    }

    public EntityAdministrator Entities { get; }
    public ComponentAdministrator Components { get; }


    public void RemoveAll()
    {
        for (var i = this.Entities.Entities.Count - 1; i >= 0; i--)
        {
            var entity = this.Entities.Entities[i];
            this.Components.MarkForRemoval(entity);
            this.Entities.Remove(entity);
        }
    }
}