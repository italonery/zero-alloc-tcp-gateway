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

/// <summary>
/// Represents the fixed 4-byte header of the binary IoT protocol.
/// </summary>
public readonly record struct MessageHeader(byte MagicByte, MessageType Type, ushort PayloadLength)
{
    /// <summary>
    /// The expected magic byte for all packets to prevent reading corrupted streams.
    /// </summary>
    public const byte ExpectedMagicByte = 0xAA;

    /// <summary>
    /// The fixed size of the header in bytes (MagicByte: 1, Type: 1, Length: 2).
    /// </summary>
    public const int Size = 4;
}
