namespace KSPMissionControl.MCP.Krpc;

public sealed class KrpcConnectionException : Exception
{
    public KrpcConnectionException(string message, Exception inner) : base(message, inner) { }
}
