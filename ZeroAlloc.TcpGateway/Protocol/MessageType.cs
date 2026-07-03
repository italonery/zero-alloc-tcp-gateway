namespace ZeroAlloc.TcpGateway.Protocol;

/// <summary>
/// Defines the telemetry messages supported by the gateway.
/// </summary>
public enum MessageType : byte
{
    Unknown = 0,
    Heartbeat = 1,
    LocationData = 2,
    SensorMetrics = 3
}
