using System.Numerics;

namespace Mini.Engine.Graphics;
public readonly struct StructTransform
{
    // TODO: implement remaining methods of Transform.cs and see if it works?

    private readonly Vector3 Position;
    private readonly Quaternion Rotation;
    private readonly Vector3 Origin;  
    private readonly float Scale;

    internal StructTransform(Vector3 position, Quaternion rotation, Vector3 origin, float scale)
    {
        this.Position = position;
        this.Rotation = rotation;
        this.Origin = origin;
        this.Scale = scale;
    }

    public Matrix4x4 GetMatrix()
    {
        var moveToCenter = Matrix4x4.CreateTranslation(-this.Origin);
        var size = Matrix4x4.CreateScale(this.Scale);
        var translation = Matrix4x4.CreateTranslation(this.Position);

        var rotationMatrix4x4 = Matrix4x4.CreateFromQuaternion(this.Rotation);
        return size * moveToCenter * rotationMatrix4x4 * translation;        
    }

    public Quaternion GetRotation()
    {
        return this.Rotation != default
            ? this.Rotation
            : Quaternion.Identity;
    }

    public Vector3 GetPosition()
    {
        return this.Position;
    }

    public Vector3 GetForward()
    {        
        return Vector3.Transform(-Vector3.UnitZ, this.GetRotation());
    }

    public Vector3 GetUp()
    {        
        return Vector3.Transform(Vector3.UnitY, this.GetRotation());
    }

    public Vector3 GetLeft()
    {        
        return Vector3.Transform(-Vector3.UnitX, this.GetRotation());
    }

    public float GetScale()
    {
        return this.Scale;
    }

    public Vector3 GetOrigin()
    {
        return this.Origin;
    }    

    public StructTransform Rotate(Quaternion offset)
    {
        return new StructTransform(this.Position, this.GetRotation() * offset, this.Origin, this.Scale);
    }

    public StructTransform RotateInReferenceFrame(Quaternion offset)
    {
        throw new NotImplementedException("TODO: how to do this?");
    }

    public StructTransform Translate(Vector3 offset)
    {
        return new StructTransform(this.Position + offset, this.GetRotation(), this.Origin, this.Scale);
    }

    public StructTransform TranslateInReferenceFrame(Vector3 offset)
    {
        var vector = Vector3.Transform(offset, this.GetRotation());
        return this.Translate(vector);
    }
    
    public StructTransform Size(float change)
    {
        return new StructTransform(this.Position, this.GetRotation(), this.Origin, this.Scale * change);
    }
}
