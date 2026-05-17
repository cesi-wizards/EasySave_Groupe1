using EasyServer.Servers;

namespace EasyServer;

internal sealed class Program
{
    private static async Task Main()
    {
        var server = new LogServer(11000);
        await server.StartAsync();
    }
}
