using System.Buffers;
using ZeroAlloc.TcpGateway.Protocol;

namespace ZeroAlloc.TcpGateway.Tests;

public class MessageParserTests
{
    [Fact]
    public void TryParseHeader_WithValidBuffer_SouldExtractCorrectly()
    {
        byte[] rawBuffer = [0xAA, 0x02, 0x02, 0x01];

        var sequence = new ReadOnlySequence<byte>(rawBuffer);
        var reader = new SequenceReader<byte>(sequence);

        bool result = MessageParser.TryParseHeader(ref reader, out var header);

        Assert.True(result);
        Assert.Equal(MessageHeader.ExpectedMagicByte, header.MagicByte);
        Assert.Equal(MessageType.LocationData, header.Type);
        Assert.Equal(258, header.PayloadLength);
    }

    [Fact]
    public void TryParseHeader_WithIncompleteBuffer_ShouldReturnFalse()
    {
        byte[] rawBuffer = [0xAA, 0x01, 0x00];
        
        var sequence = new ReadOnlySequence<byte>(rawBuffer);
        var reader = new SequenceReader<byte>(sequence);

        bool result = MessageParser.TryParseHeader(ref reader, out var header);

        Assert.False(result);
        Assert.Equal(default, header);
    }

    [Fact]
    public void TryParseHeader_WithInvalidMagicByte_ShouldReturnFalse()
    {
        byte[] rawBuffer = [0xBB, 0x01, 0x05, 0x00];
        
        var sequence = new ReadOnlySequence<byte>(rawBuffer);
        var reader = new SequenceReader<byte>(sequence);

        bool result = MessageParser.TryParseHeader(ref reader, out _);

        Assert.False(result);
    }
}
