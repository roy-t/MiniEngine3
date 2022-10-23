namespace Mini.Engine.Content.v2;
public record ContentHeader(Guid Type, DateTime Timestamp, ContentRecord Meta, ISet<string> Dependencies);
