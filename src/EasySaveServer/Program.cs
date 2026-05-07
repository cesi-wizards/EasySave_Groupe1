using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasySaveServer;
internal sealed class Program
{
    private static async Task Main()
    {
        int port = 11000;
        var listener = new TcpListener(IPAddress.Any, port);

        try
        {
            listener.Start();
            Console.WriteLine($"[*] Centralised Log Server listening on port : {port}...");

            while (true)
            {
                // Waiting for a connection in an asynchronous method
                TcpClient client = await listener.AcceptTcpClientAsync();

                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Server Error]: {ex.Message}");
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        using (client)
        await using (NetworkStream stream = client.GetStream())
        {
            try
            {
                // Read the full stream until the client closes the connection
                byte[] buffer = new byte[8192];
                using var ms = new MemoryStream();
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await ms.WriteAsync(buffer, 0, bytesRead);
                }

                string message = Encoding.UTF8.GetString(ms.ToArray());

                if (string.IsNullOrWhiteSpace(message))
                {
                    await SendResponseAsync(stream, "ERROR:EMPTY_MESSAGE");
                    return;
                }

                Console.WriteLine($"[Log @{DateTime.Now}] : {message}");

                Dictionary<string, object> content;
                try
                {
                    content = System.Text.Json.JsonSerializer
                                  .Deserialize<Dictionary<string, object>>(message)
                              ?? new Dictionary<string, object> { { "raw", message } };
                }
                catch (System.Text.Json.JsonException)
                {
                    await SendResponseAsync(stream, "ERROR:INVALID_JSON");
                    return;
                }

                string logType = "json";
                if (content.TryGetValue("LogType", out object? logTypeValue))
                {
                    logType = logTypeValue?.ToString() ?? "json";
                }

                EasyLog.EasyLog.Instance.Write(GetLogFilePath(), content, logType);

                await SendResponseAsync(stream, "OK");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[Socket error]: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
                try
                {
                    await SendResponseAsync(stream, "ERROR:INTERNAL");
                }
                catch
                {
                    Console.WriteLine($"[ERROR] couldn't notify client of failure");
                }
            }
        }
    }

    /// <summary>
    /// Sends a response code back to the client
    /// </summary>
    /// <param name="stream"> The network stream to write to </param>
    /// <param name="response"> The response code to send (OK, ERROR:...) </param>
    private static async Task SendResponseAsync(NetworkStream stream, string response)
    {
        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    private static string GetLogFilePath()
    {
        string folderName = "logs";
        string fileName = $"{DateTime.Now:yyyy-MM-dd}";
        return Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);
    }
}
