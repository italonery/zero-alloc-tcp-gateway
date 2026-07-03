using System.Buffers;

namespace ZeroAlloc.TcpGateway.Protocol;

/// <summary>
/// Parser for extracting binary protocol headers from memory sequences.
/// </summary>
public static class MessageParser
{
    /// <summary>
    /// Attempts to parse the 4-byte header from the current position in the sequence.
    /// </summary>
    /// <param name="reader">The sequence reader pointing to the network buffer.</param>
    /// <param name="header">The extracted header if successful.</param>
    /// <returns>True if the full header was available and valid; otherwise, false.</returns>
    public static bool TryParseHeader(ref SequenceReader<byte> reader, out MessageHeader header)
    {
        header = default;
        
        if (reader.Remaining < MessageHeader.Size)
        {
            return false;
        }

        if (!reader.TryRead(out byte magicByte) || magicByte != MessageHeader.ExpectedMagicByte)
        {
            return false;
        }

        reader.TryRead(out byte typeByte);
        reader.TryReadLittleEndian(out short payloadLenghtSigned);

        header = new MessageHeader(magicByte, (MessageType)typeByte, (ushort)payloadLenghtSigned);

        return true;
    }
}
