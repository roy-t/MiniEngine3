using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public class VertexShader : Shader<ID3D11VertexShader>
    {
        public VertexShader(Device device, string fileName, string entryPoint, string profile)
            : base(device, fileName, entryPoint, profile) { }

        protected override ID3D11VertexShader Create(Blob blob)
            => this.Device.ID3D11Device.CreateVertexShader(blob.GetBytes());
    }
}
