using System.Net.Sockets;
using ZeroAlloc.TcpGateway.Networking;

namespace ZeroAlloc.TcpGateway.Simulator;

public static class Program
{
    public static async Task Main()
    {
        Console.WriteLine("=== TCP Zero-Allocation Gateway Simulator ===");

        var server = new GatewayServer(5000);
        server.Start();

        await Task.Delay(500);

        Console.WriteLine("\n[Client] Connecting to server...");
        using var client = new TcpClient("127.0.0.1", 5000);
        await using var stream = client.GetStream();

        byte[] header = [0xAA, 0x02, 0x0A, 0x00];
        byte[] payload = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];

        Console.WriteLine("\n[Client] Sending 3 perfect packets...");
        for (int i = 1; i <= 3; i++)
        {
            await stream.WriteAsync(header);
            await stream.WriteAsync(payload);
            await Task.Delay(100);
        }

        Console.WriteLine("\n[Client] Simulating severe TCP fragmentation...");
        
        byte[] fragment1 = [0xAA, 0x02];
        await stream.WriteAsync(fragment1);
        Console.WriteLine("[Client] Sent half of the header. Delaying 2 seconds...");
        
        await Task.Delay(2000); 

        byte[] fragment2 = [0x0A, 0x00, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1];
        await stream.WriteAsync(fragment2);
        Console.WriteLine("[Client] Sent the remaining bytes.");

        await Task.Delay(1000);
        
        Console.WriteLine("\n[Client] Disconnecting...");
        server.Stop();
    }
}
