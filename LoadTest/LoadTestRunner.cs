using System.Net.WebSockets;
using System.Text;

namespace LoadTest;

class LoadTestRunner
{
    static async Task Main()
    {
        int clients = 10;
        int messagesPerClient = 5;

        Console.WriteLine($"Starting load test with {clients} clients, {messagesPerClient} messages each...\n");

        var tasks = Enumerable.Range(0, clients).Select(RunClient).ToArray();
        await Task.WhenAll(tasks);

        Console.WriteLine("\nLoad test complete.");
    }

    static async Task RunClient(int clientId)
    {
        try
        {
            using var socket = new ClientWebSocket();
            await socket.ConnectAsync(new Uri("ws://localhost:5000/ws/"), CancellationToken.None);

            for (int i = 0; i < 5; i++)
            {
                var request = new
                {
                    Count = 3,
                    Length = 8
                };

                var payload = Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(request));
                await socket.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Text, true, CancellationToken.None);

                var buffer = new byte[1024];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string response = Encoding.UTF8.GetString(buffer, 0, result.Count);

                Console.WriteLine($"[Client {clientId}] Response {i + 1}: {response}");
            }

            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Client {clientId}] Error: {ex.Message}");
        }
    }
}
