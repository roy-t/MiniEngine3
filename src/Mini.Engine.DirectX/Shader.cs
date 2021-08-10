﻿using System;
using System.IO;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public abstract class Shader<TShader> : IDisposable
        where TShader : ID3D11DeviceChild
    {
        private static readonly ShaderMacro[] Defines = Array.Empty<ShaderMacro>();

        protected readonly Device Device;
        private Blob blob;

        public Shader(Device device, string fileName, string entryPoint, string profile)
        {
            this.Device = device;
            this.FileName = fileName;
            this.FullPath = Path.GetFullPath(fileName);
            this.EntryPoint = entryPoint;
            this.Profile = profile;

            this.Reload();
        }

        internal TShader ID3D11Shader { get; set; }

        public string FileName { get; }
        public string FullPath { get; }
        public string EntryPoint { get; }
        public string Profile { get; }

        public void Reload()
        {
            // Files are read via .NET methods
            var sourceText = File.ReadAllText(this.FullPath);
            using var include = new ShaderFileInclude(Path.GetDirectoryName(this.FullPath));

            Compiler.Compile(sourceText, Defines, include, this.EntryPoint, this.FileName, this.Profile, out this.blob, out var vsErrorBlob);
            this.ID3D11Shader = this.Create(this.blob);
        }

        protected abstract TShader Create(Blob blob);

        public InputLayout CreateInputLayout(params InputElementDescription[] elements)
            => new(this.Device.ID3D11Device.CreateInputLayout(elements, this.blob));

        public void Dispose()
        {
            this.blob?.Dispose();
            this.ID3D11Shader?.Dispose();

            GC.SuppressFinalize(this);
        }
    }
}
