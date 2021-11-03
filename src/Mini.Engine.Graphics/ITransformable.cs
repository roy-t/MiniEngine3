using System.Numerics;

namespace Mini.Engine.Graphics
{
    public interface ITransformable<T>
    {
        public Transform Transform { get; }
        public void OnTransform();
    }

    public static class ITransformExtensions
    {
        public static void MoveTo<T>(this ITransformable<T> target, Vector3 position)
        {
            target.Transform.MoveTo(position);
            target.OnTransform();
        }

        public static void SetScale<T>(this ITransformable<T> target, float scale)
        {
            target.Transform.SetScale(scale);
            target.OnTransform();
        }

        public static void SetScale<T>(this ITransformable<T> target, Vector3 scale)
        {
            target.Transform.SetScale(scale);
            target.OnTransform();
        }

        public static void SetOrigin<T>(this ITransformable<T> target, Vector3 origin)
        {
            target.Transform.SetOrigin(origin);
            target.OnTransform();
        }

        public static void SetRotation<T>(this ITransformable<T> target, Quaternion rotation)
        {
            target.Transform.SetRotation(rotation);
            target.OnTransform();
        }

        public static void ApplyTranslation<T>(this ITransformable<T> target, Vector3 translation)
        {
            target.Transform.MoveTo(target.Transform.Position + translation);
            target.OnTransform();
        }

        public static void ApplyRotation<T>(this ITransformable<T> target, Quaternion rotation)
        {
            target.Transform.ApplyRotation(rotation);
            target.OnTransform();
        }

        public static void FaceTarget<T>(this ITransformable<T> target, Vector3 position)
        {
            target.Transform.FaceTarget(position);
            target.OnTransform();
        }

        public static void FaceTargetConstrained<T>(this ITransformable<T> target, Vector3 position, Vector3 up)
        {
            target.Transform.FaceTargetConstrained(position, up);
            target.OnTransform();
        }
    }
}
