﻿namespace Mini.Engine.Content.v2;
public interface IContent
{
    ContentId Id { get; }
    ContentRecord Meta { get; }
    string GeneratorKey { get; }
    ISet<string> Dependencies { get; }
    void Dispose();
}
