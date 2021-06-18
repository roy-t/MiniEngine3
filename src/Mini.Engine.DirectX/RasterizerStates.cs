﻿using System;
using Vortice.Direct3D11;

namespace Mini.Engine.DirectX
{
    public sealed class RasterizerState : IDisposable
    {
        internal RasterizerState(ID3D11RasterizerState state, string name)
        {
            this.State = state;
            this.State.DebugName = name;
            this.Name = name;
        }

        public string Name { get; }

        internal ID3D11RasterizerState State { get; }

        public void Dispose()
            => this.State.Dispose();
    }

    public sealed class RasterizerStates : IDisposable
    {
        internal RasterizerStates(ID3D11Device device)
        {
            this.CullNone = Create(device, CullNoneDescription(), nameof(this.CullNone));
        }

        public RasterizerState CullNone { get; }

        private static RasterizerState Create(ID3D11Device device, RasterizerDescription description, string name)
        {
            var state = device.CreateRasterizerState(description);
            return new RasterizerState(state, name);
        }

        private static RasterizerDescription CullNoneDescription()
        {
            return new RasterizerDescription
            {
                FillMode = FillMode.Solid,
                CullMode = CullMode.None,
                ScissorEnable = true,
                DepthClipEnable = true
            };
        }

        public void Dispose()
        {
            this.CullNone.Dispose();
        }
    }
}