namespace Mini.Engine.Content.v2;
public record ContentHeader(Guid Type, int Version, DateTime Timestamp, ISet<string> Dependencies);
