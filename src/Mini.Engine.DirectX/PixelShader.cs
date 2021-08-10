using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public class PixelShader : Shader<ID3D11PixelShader>
    {
        public PixelShader(Device device, string fileName, string entryPoint, string profile)
            : base(device, fileName, entryPoint, profile) { }

        protected override ID3D11PixelShader Create(Blob blob)
            => this.Device.ID3D11Device.CreatePixelShader(blob.GetBytes());
    }
}
