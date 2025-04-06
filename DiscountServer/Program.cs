using DiscountServer;
using Microsoft.Extensions.Configuration;

var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cts.Cancel();
};

try
{
    var config = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .Build();

    var uri = config.GetSection("WebSocketSettings:Uri").Value;

    if (string.IsNullOrEmpty(uri))
    {
        throw new InvalidOperationException("WebSocketSettings:Uri is not configured.");
    }

    var server = new WebSocketServer(uri);
    await server.StartAsync(cts.Token);
}
finally
{
    cts.Dispose();
}
