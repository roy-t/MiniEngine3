using System.Numerics;
using Mini.Engine.ECS;
using Mini.Engine.ECS.Components;

namespace Mini.Engine.Graphics;
public struct CameraComponent : IComponent
{
    public PerspectiveCamera Camera;

    public Entity Entity { get; set; }
    public LifeCycle LifeCycle { get; set; }

    public void Init(float aspectRatio)
    {
        var transform = Transform.Identity;
        transform.MoveTo(new Vector3(0, 0, 0));
        transform.FaceTargetConstrained(-Vector3.UnitZ, Vector3.UnitY);
        this.Camera = new PerspectiveCamera(aspectRatio, transform);
    }

    public void Destroy()
    {
        
    }
}
