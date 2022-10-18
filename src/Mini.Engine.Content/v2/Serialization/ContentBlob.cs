namespace Mini.Engine.Content.v2.Serialization;

internal record ContentBlob(Guid Header, ContentRecord Meta, IReadOnlyList<string> Dependencies, byte[] Contents);