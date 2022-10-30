namespace Mini.Engine.Content.v2.Serialization;
public record ContentHeader(Guid Type, int Version, DateTime Timestamp, ISet<string> Dependencies);
