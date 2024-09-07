namespace MultiplayerDemo;
public record class Message(int SourceId, int DestinationId, int Step, double TargetDt, int Action);
