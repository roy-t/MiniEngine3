using Serilog.Events;

namespace Mini.Engine.DirectX.Debugging;

internal sealed record class Message(string Description, LogEventLevel Level);
