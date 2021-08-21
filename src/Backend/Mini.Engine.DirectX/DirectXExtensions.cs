using System.Runtime.InteropServices;
using Vortice;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    internal static class DirectXExtensions
    {
        public static void SetName(this ID3D11Device device, string name)
        {
            var ptr = Marshal.StringToHGlobalAnsi(name);
            device.SetPrivateData(CommonGuid.DebugObjectName, name.Length, ptr);
        }

        public static void SetName(this ID3D11DeviceChild child, string name)
        {
            var ptr = Marshal.StringToHGlobalAnsi(name);
            child.SetPrivateData(CommonGuid.DebugObjectName, name.Length, ptr);
        }
    }
}
