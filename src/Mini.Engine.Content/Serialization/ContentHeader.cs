namespace Mini.Engine.Content.Serialization;
public record ContentHeader(Guid Type, int Version, DateTime Timestamp, ISet<string> Dependencies);
