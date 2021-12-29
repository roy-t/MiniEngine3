﻿using System;

namespace Mini.Engine.DirectX.Resources;

public interface IModel : IDisposable
{
    VertexBuffer<ModelVertex> Vertices { get; }
    IndexBuffer<int> Indices { get; }
    Primitive[] Primitives { get; }

    IMaterial[] Materials { get; }
}