﻿using Mini.Engine.DirectX.Resources.Surfaces;

namespace Mini.Engine.DirectX.Resources;

public interface IMaterial : IDeviceResource
{
    public ISurface Albedo { get; }
    public ISurface Metalicness { get; }
    public ISurface Normal { get; }
    public ISurface Roughness { get; }
    public ISurface AmbientOcclusion { get; }
}
