namespace Mini.Engine.Content.v2.Serialization;

internal record ContentBlob(Guid Header, DateTime Timestamp, ContentRecord Meta, IReadOnlyList<string> Dependencies, byte[] Contents);