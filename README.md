# Zero-Allocation TCP Gateway

Designed to handle continuous binary telemetry streams (e.g., IoT sensors, vehicle trackers) while completely eliminating Garbage Collector (GC) pressure using `System.IO.Pipelines`.

Traditional TCP servers in C# often rely on allocating new `byte[]` arrays for every incoming network read. In high-throughput scenarios (thousands of devices sending telemetry every second), this naive approach causes massive Heap allocations, forcing the Garbage Collector to freeze the CPU to clean up short-lived objects.

Additionally, standard servers often fail to properly handle **TCP Fragmentation**—where a single logical message is split across multiple network packets.

## Architecture

This gateway solves these critical infrastructure problems by leveraging modern .NET memory primitives:

* **`System.IO.Pipelines`:** Replaces standard stream reading with a producer-consumer model that recycles contiguous blocks of memory from an internal `ArrayPool`.
* **`ReadOnlySequence<byte>`:** Acts as a virtual window over the network buffers, allowing us to slice and inspect TCP payloads without ever copying them into a new array.
* **`SequenceReader<byte>`:** Traverses the memory segments to safely extract binary headers, enforcing strict *Little-Endian* byte-order decoding while remaining completely allocated on the Stack (`ref struct`).

## Binary Protocol Specification

To handle framing efficiently, the gateway enforces a strict 4-byte zero-allocation header for all incoming streams:

| Offset | Size    | Type     | Description |
| :---   | :---    | :---     | :--- |
| `0`    | 1 byte  | `byte`   | **Magic Byte:** Fixed at `0xAA` to reject network noise/port scans. |
| `1`    | 1 byte  | `byte`   | **Message Type:** Telemetry category (1 = Heartbeat, 2 = Location). |
| `2`    | 2 bytes | `ushort` | **Payload Length:** Size of the upcoming payload (Little-Endian). |

## Testing TCP Fragmentation

The repository includes a byte-level `xUnit` test suite to mathematically prove Little-Endian parsing, as well as a real-time `Simulator` project.

The simulator explicitly generates severe TCP fragmentation by writing half a packet, pausing the thread, and writing the rest. The `GatewayServer` catches the incomplete buffer, waits without allocating memory, and perfectly slices the payload once the remaining bytes arrive.

**Run the Simulator:**
```bash
dotnet run --project ZeroAlloc.TcpGateway.Simulator/ZeroAlloc.TcpGateway.Simulator.csproj
```

## License
MIT © [Ítalo Nery](https://github.com/italonery)