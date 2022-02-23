using System.Numerics;
using Mini.Engine.ECS;

namespace Mini.Engine.Graphics;
public sealed class CameraComponent : Component
{
    public CameraComponent(Entity entity, float aspectRatio)
        : base(entity)
    {
        var transform = Transform.Identity;
        transform.MoveTo(new Vector3(0, 0, 0));
        transform.FaceTargetConstrained(-Vector3.UnitZ, Vector3.UnitY);
        this.Camera = new PerspectiveCamera(aspectRatio, transform);
    }

    public PerspectiveCamera Camera { get; }
}
