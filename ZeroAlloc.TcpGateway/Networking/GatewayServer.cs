using System.Buffers;
using System.IO.Pipelines;
using System.Net.Sockets;
using ZeroAlloc.TcpGateway.Protocol;

namespace ZeroAlloc.TcpGateway.Networking;

/// <summary>
/// A high-performance, zero-allocation TCP server built on top of System.IO.Pipelines.
/// </summary>
public class GatewayServer
{
    private readonly int _port;
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;

    public GatewayServer(int port)
    {
        _port = port;
    }

    /// <summary>
    /// Starts the TCP server and begins accepting incoming connections asynchronously.
    /// </summary>
    public void Start()
    {
        _cts = new CancellationTokenSource();
        _listener = new TcpListener(System.Net.IPAddress.Any, _port);
        _listener.Start();

        Console.WriteLine($"[Gateway] Listening on port {_port}...");

        _ = AcceptConnectionsAsync(_cts.Token);
    }

    /// <summary>
    /// Gracefully shuts down the gateway.
    /// </summary>
    public void Stop()
    {
        _cts?.Cancel();
        _listener?.Stop();
        Console.WriteLine("[Gateway] stopped.");
    }

    private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                Console.WriteLine($"[Gateway] Client connected: {client.Client.RemoteEndPoint}.");

                _ = ProcessClientAsync(client, cancellationToken);
            }
        }
        catch (OperationCanceledException) { }
    }

    private async Task ProcessClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        using (client)
        {
            var stream = client.GetStream();

            var reader = PipeReader.Create(stream, new StreamPipeReaderOptions(leaveOpen: false));

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    ReadResult result = await reader.ReadAsync(cancellationToken);
                    ReadOnlySequence<byte> buffer = result.Buffer;

                    SequencePosition consumed = buffer.Start;
                    SequencePosition examined = buffer.End;

                    try
                    {
                        if (result.IsCanceled)
                        {
                            break;
                        }

                        while (TryParseMessage(ref buffer, out consumed))
                        {
                            
                        }

                        examined = buffer.Start;
                    }
                    finally
                    {
                        reader.AdvanceTo(consumed, examined);
                    }

                    if (result.IsCompleted)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Gateway] Client error: {ex.Message}");
            }
            finally
            {
                await reader.CompleteAsync();
                Console.WriteLine("[Gateway] Client disconnected.");
            }
        }
    }

    /// <summary>
    /// Attempts to slice a full protocol message from network buffer.
    /// </summary>
    private bool TryParseMessage(ref ReadOnlySequence<byte> buffer, out SequencePosition consumed)
    {
        consumed = buffer.Start;
        var sequenceReader = new SequenceReader<byte>(buffer);

        if (!MessageParser.TryParseHeader(ref sequenceReader, out var header))
        {
            return false;
        }

        if (sequenceReader.Remaining < header.PayloadLength)
        {
            return false;
        }

        var payloadSequence = buffer.Slice(sequenceReader.Position, header.PayloadLength);

        // TODO: RabbitMQ, Kafka, or other message broker integration can be added here to forward the payload.
        Console.WriteLine($"[Gateway] Processed '{header.Type}' | Payload: {header.PayloadLength} bytes.");

        consumed = buffer.GetPosition(MessageHeader.Size + header.PayloadLength);

        buffer = buffer.Slice(consumed);

        return true;
    }
}
