using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyServer.Servers;

public sealed class LogServer(int port)
{
    private readonly TcpListener _listener = new TcpListener(IPAddress.Any, port);
    private bool _isRunning;

    public async Task StartAsync(CancellationToken ct = default)
    {
        _listener.Start();
        _isRunning = true;
        Debug.WriteLine($"[*] Centralised Log Server listening on port : {port}...");

        try
        {
            while (!ct.IsCancellationRequested && _isRunning)
            {
                TcpClient client = await _listener.AcceptTcpClientAsync(ct);
                _ = Task.Run(() => HandleClientAsync(client), ct);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine($"Operation canceled");
        }
        finally { _listener.Stop(); }
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

                Debug.WriteLine($"[Log @{DateTime.Now}] : {message}");

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
                Debug.WriteLine($"[Socket error]: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR]: {ex.Message}");
                try
                {
                    await SendResponseAsync(stream, "ERROR:INTERNAL");
                }
                catch
                {
                    Debug.WriteLine($"[ERROR] couldn't notify client of failure");
                }
            }
        }
    }

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
