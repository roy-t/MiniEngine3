namespace Mini.Engine.Content.v2.Serialization;

public sealed record ContentBlob(Guid Header, DateTime Timestamp, ContentRecord Meta, ISet<string> Dependencies, byte[] Contents);