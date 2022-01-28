using System;
using System.Numerics;
using Mini.Engine.DirectX.Contexts;
using Mini.Engine.DirectX.Resources;

namespace Mini.Engine.Graphics.Textures.Generators;

public static class CubeMap
{
    public static readonly CubeMapFace[] Faces = Enum.GetValues<CubeMapFace>();
    
    public static void RenderFaces(DeviceContext context, FullScreenTriangle fullScreenTriangle, RenderTargetCube target, ICubeMapRenderer renderer)
    {
        context.IA.SetVertexBuffer(fullScreenTriangle.Vertices);
        context.IA.SetIndexBuffer(fullScreenTriangle.Indices);

        var projection = Matrix4x4.CreatePerspectiveFieldOfView(MathF.PI / 2.0f, 1.0f, 0.1f, 1.5f);

        for (var i = 0; i < Faces.Length; i++)
        {
            var face = Faces[i];
            var view = GetViewMatrixForFace(face);
            var worldViewProjection = view * projection;
            Matrix4x4.Invert(worldViewProjection, out var inverse);

            renderer.SetInverseViewProjection(inverse);
            
            context.OM.SetRenderTarget(target, face);
            context.DrawIndexed(FullScreenTriangle.PrimitiveCount, FullScreenTriangle.PrimitiveOffset, FullScreenTriangle.VertexOffset);
        }
    }

    public static Matrix4x4 GetViewMatrixForFace(CubeMapFace face)
    {
        return face switch
        {
            CubeMapFace.PositiveX => Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitX, Vector3.UnitY),
            CubeMapFace.NegativeX => Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitX, Vector3.UnitY),
            CubeMapFace.PositiveY => Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ),
            CubeMapFace.NegativeY => Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitY, -Vector3.UnitZ),
            // Invert Z as we assume a left handed (DirectX 9) coordinate system in the source texture
            CubeMapFace.PositiveZ => Matrix4x4.CreateLookAt(Vector3.Zero, -Vector3.UnitZ, Vector3.UnitY),
            CubeMapFace.NegativeZ => Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY),
            _ => throw new ArgumentOutOfRangeException(nameof(face))
        };
    }
}

public interface ICubeMapRenderer
{
    void SetInverseViewProjection(Matrix4x4 inverseViewProjection);
}
