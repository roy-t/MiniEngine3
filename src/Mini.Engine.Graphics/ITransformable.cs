using System.Numerics;

namespace Mini.Engine.Graphics
{
    public interface ITransformable<T>
    {
        public Transform Transform { get; }
        public T Retransform(Transform transform);
    }

    public static class ITransformExtensions
    {
        public static T MoveTo<T>(this ITransformable<T> target, Vector3 position)
        {
            return target.Retransform(target.Transform.MoveTo(position));
        }

        public static T SetScale<T>(this ITransformable<T> target, float scale)
        {
            return target.Retransform(target.Transform.SetScale(scale));
        }

        public static T SetScale<T>(this ITransformable<T> target, Vector3 scale)
        {
            return target.Retransform(target.Transform.SetScale(scale));
        }

        public static T SetOrigin<T>(this ITransformable<T> target, Vector3 origin)
        {
            return target.Retransform(target.Transform.SetOrigin(origin));
        }

        public static T SetRotation<T>(this ITransformable<T> target, Quaternion rotation)
        {
            return target.Retransform(target.Transform.SetRotation(rotation));
        }

        public static T ApplyRotation<T>(this ITransformable<T> target, Quaternion rotation)
        {
            return target.Retransform(target.Transform.ApplyRotation(rotation));
        }

        public static T FaceTarget<T>(this ITransformable<T> target, Vector3 position)
        {
            return target.Retransform(target.Transform.FaceTarget(position));
        }

        public static T FaceTargetConstrained<T>(this ITransformable<T> target, Vector3 position, Vector3 up)
        {
            return target.Retransform(target.Transform.FaceTargetConstrained(position, up));
        }
    }
}
