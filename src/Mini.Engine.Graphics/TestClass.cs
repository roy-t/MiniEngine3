/// <generated>
/// SunLight.cs
/// </generated>

namespace Mini.Engine.Content.Shaders.Generated
{
    public sealed class SunLight
    {
        public const int TextureSampler = 0;
        public const int Albedo = 0;
        public const int Material = 1;
        public const int Depth = 2;
        public const int Normal = 3;
        public const int ShadowMap = 4;
        public const int ShadowSampler = 1;

        private readonly Mini.Engine.DirectX.Device Device;
        private readonly Mini.Engine.IO.IVirtualFileSystem FileSystem;
        private readonly Mini.Engine.Content.ContentManager Content;

        public SunLight(Mini.Engine.DirectX.Device device, Mini.Engine.IO.IVirtualFileSystem fileSystem, Mini.Engine.Content.ContentManager content)
        {
            this.Device = device;
            this.FileSystem = fileSystem;
            this.Content = content; this.Ps = new Mini.Engine.Content.Shaders.PixelShaderContent(this.Device, this.FileSystem, this.Content, new Mini.Engine.Content.ContentId("Shaders\\Lighting\\SunLight.hlsl", "PS"), "ps_5_0"); ;

        }

        public Mini.Engine.DirectX.Resources.IPixelShader Ps { get; }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct Mat
        {
            public float Metalicness { get; set; }
            public float Roughness { get; set; }
            public float AmbientOcclusion { get; set; }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct ShadowProperties
        {
            public System.Numerics.Matrix4x4 ShadowMatrix { get; set; }
            public System.Numerics.Matrix4x4 Offsets { get; set; }
            public System.Numerics.Matrix4x4 Scales { get; set; }
            public System.Numerics.Vector4 Splits { get; set; }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct PS_INPUT
        {
            public System.Numerics.Vector4 Pos { get; set; }
            public System.Numerics.Vector2 Tex { get; set; }
        }

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 4)]
        public struct Constants
        {
            public System.Numerics.Vector4 Color { get; set; }
            public System.Numerics.Vector3 SurfaceToLight { get; set; }
            public float Strength { get; set; }
            public System.Numerics.Matrix4x4 InverseViewProjection { get; set; }
            public System.Numerics.Vector3 CameraPosition { get; set; }
            public float Unused { get; set; }
            public ShadowProperties Shadow { get; set; }
        }

        public sealed class User : System.IDisposable
        {
            private readonly Mini.Engine.DirectX.Buffers.ConstantBuffer<Constants> ConstantsBuffer;

            public User(Mini.Engine.DirectX.Device device)
            {
                this.ConstantsBuffer = new Mini.Engine.DirectX.Buffers.ConstantBuffer<Constants>(device, "SunLight_Buffers_CB");

            }

            public void MapConstants(Mini.Engine.DirectX.Contexts.DeviceContext context, System.Numerics.Vector4 color, System.Numerics.Vector3 surfaceToLight, float strength, System.Numerics.Matrix4x4 inverseViewProjection, System.Numerics.Vector3 cameraPosition, float unused, ShadowProperties shadow)
            {
                var constants = new Constants()
                {
                    Color = color,
                    SurfaceToLight = surfaceToLight,
                    Strength = strength,
                    InverseViewProjection = inverseViewProjection,
                    CameraPosition = cameraPosition,
                    Unused = unused,
                    Shadow = shadow
                };

                this.ConstantsBuffer.MapData(context, constants);
            }

            public void Dispose()
            {
                this.ConstantsBuffer.Dispose();

            }
        }
    }
}