using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Text.Json;
using DiscountServer.Storage;
using DiscountServer.Models;


namespace DiscountServer;

public class WebSocketServer
{
    private readonly HttpListener _listener;
    private readonly DiscountService _service;

    public WebSocketServer(string url)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(url);
        _service = new DiscountService(new DiscountStorage("discounts.db"));
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Console.WriteLine("WebSocket server listening...");

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await _listener.GetContextAsync().WaitAsync(cancellationToken);

                if (context.Request.IsWebSocketRequest)
                {
                    _ = HandleConnectionAsync(context); // fire-and-forget
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server shutdown requested.");
        }
        finally
        {
            _listener.Stop();
        }
    }


    private async Task HandleConnectionAsync(HttpListenerContext context)
    {
        var wsContext = await context.AcceptWebSocketAsync(null);
        var socket = wsContext.WebSocket;
        var buffer = new byte[1024];


        while (socket.State == WebSocketState.Open)
        {
            Console.WriteLine($"[Receiving] Thread {Environment.CurrentManagedThreadId}, Task {Task.CurrentId ?? -1}, Time: {DateTime.Now:T}");

            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
                break;

            string json = Encoding.UTF8.GetString(buffer, 0, result.Count);
            string responseJson = HandleRequest(json);

            byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);
            await socket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
    }

    private string HandleRequest(string json)
    {
        if (json.Contains("Count"))
        {
            var req = JsonSerializer.Deserialize<GenerateRequest>(json);

            if (req == null || req.Count == 0 || req.Length == 0)
            {
                return JsonSerializer.Serialize(new { Error = "Count and Length must be greater than 0" });
            }

            var codes = _service.GenerateCodes(req.Count, req.Length);
            var res = new GenerateResponse
            {
                Result = codes.Count > 0,
                Codes = codes
            };

            return JsonSerializer.Serialize(res);
        }

        if (json.Contains("Code"))
        {
            var req = JsonSerializer.Deserialize<UseCodeRequest>(json);
            if (req == null)
            {
                return JsonSerializer.Serialize(new { Error = "Invalid request" });
            }
            var res = new UseCodeResponse { Result = _service.UseCode(req.Code) };
            return JsonSerializer.Serialize(res);
        }

        if (json.Contains("GetAll"))
        {
            var all = _service.GetAllCodes();
            var response = new AllCodesResponse
            {
                AllCodes = [.. all.Select(kvp => new CodeEntry
                {
                    Code = kvp.Key,
                    Used = kvp.Value
                })]
            };
            return JsonSerializer.Serialize(response);
        }

        return "{}";
    }
}
