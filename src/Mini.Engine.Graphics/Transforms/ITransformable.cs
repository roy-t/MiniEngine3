//using System.Numerics;

//namespace Mini.Engine.Graphics.Transforms;

//public interface ITransformable<T>
//{
//    public StructTransform Transform { get; set; }
//    public T OnTransform();
//}
//// TODO: remove!!
//public static class ITransformExtensions
//{
//    public static T MoveTo<T>(this ITransformable<T> target, Vector3 position)
//    {
//        target.Transform = target.Transform.SetTranslation(position);
//        return target.OnTransform();
//    }

//    public static T SetScale<T>(ref this ITransformable<T> target, float scale)
//        where T : struct
//    {
//        target.Transform = target.Transform.SetScale(scale);
//        return target.OnTransform();
//    }

//    public static T SetOrigin<T>(this ITransformable<T> target, Vector3 origin)
//    {
//        target.Transform = target.Transform.SetOrigin(origin);
//        return target.OnTransform();
//    }

//    public static T SetRotation<T>(this ITransformable<T> target, Quaternion rotation)
//    {
//        target.Transform = target.Transform.SetRotation(rotation);
//        return target.OnTransform();
//    }

//    public static T ApplyTranslation<T>(this ITransformable<T> target, Vector3 translation)
//    {
//        target.Transform = target.Transform.AddTranslation(translation);
//        return target.OnTransform();
//    }

//    public static T ApplyRotation<T>(this ITransformable<T> target, Quaternion rotation)
//    {
//        target.Transform = target.Transform.AddLocalRotation(rotation);
//        return target.OnTransform();
//    }

//    public static T FaceTarget<T>(this ITransformable<T> target, Vector3 position)
//    {
//        target.Transform = target.Transform.FaceTarget(position);
//        return target.OnTransform();
//    }

//    public static T FaceTargetConstrained<T>(this ITransformable<T> target, Vector3 position, Vector3 up)
//    {
//        target.Transform = target.Transform.FaceTargetConstrained(position, up);
//        return target.OnTransform();
//    }

//    public static Matrix4x4 AsMatrix<T>(this ITransformable<T> target)
//    {
//        return target.Transform.GetMatrix();
//    }
//}
