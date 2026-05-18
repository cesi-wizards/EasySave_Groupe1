using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EasyServer.Servers;

public sealed class LogServer(int port)
{
    private readonly TcpListener _listener = new TcpListener(IPAddress.Any, port);
    private bool _isRunning;

    private const int _maxMessageSize = 1048576;

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
                _ = Task.Run(() => HandleClientAsync(client, ct), ct);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine("Operation canceled");
        }
        finally
        {
            _listener.Stop();
        }
    }

    /// <summary>
    /// Handles a persistent client connection — reads messages in a loop until disconnection.
    /// Protocol: [4 bytes big-endian length][UTF-8 JSON payload]
    /// </summary>
    private static async Task HandleClientAsync(TcpClient client, CancellationToken ct)
    {
        using (client)
        {
            await using NetworkStream stream = client.GetStream();

            Debug.WriteLine($"[+] Client connected: {client.Client.RemoteEndPoint}");

            try
            {
                // Loop: keep reading messages from this client until it disconnects
                while (!ct.IsCancellationRequested)
                {
                    // --- Read 4-byte length header
                    byte[] headerBuffer = new byte[4];
                    bool connected = await ReadExactAsync(stream, headerBuffer, ct);
                    if (!connected) break; // client disconnected cleanly

                    if (BitConverter.IsLittleEndian) Array.Reverse(headerBuffer);
                    int messageLength = BitConverter.ToInt32(headerBuffer);

                    // --- Guard against oversized messages
                    if (messageLength <= 0 || messageLength > _maxMessageSize)
                    {
                        await SendResponseAsync(stream, "ERROR:MESSAGE_TOO_LARGE");
                        continue;
                    }

                    // --- Read exactly <messageLength> bytes
                    byte[] messageBuffer = new byte[messageLength];
                    connected = await ReadExactAsync(stream, messageBuffer, ct);
                    if (!connected) break;

                    string message = Encoding.UTF8.GetString(messageBuffer);
                    Debug.WriteLine($"[Log @{DateTime.Now}] : {message}");

                    // --- Deserialize
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
                        continue;
                    }

                    // --- Extract log type
                    string logType = "json";
                    if (content.TryGetValue("LogType", out object? logTypeValue))
                        logType = logTypeValue.ToString() ?? "json";

                    // --- Write to file
                    EasyLog.EasyLog.Instance.Write(GetLogFilePath(), content, logType);

                    // --- Acknowledge
                    await SendResponseAsync(stream, "OK");
                }
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
                    /* ignored */
                }
            }

            Debug.WriteLine($"[-] Client disconnected: {client.Client.RemoteEndPoint}");

        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads exactly buffer.Length bytes. Returns false if the connection was closed cleanly.
    /// </summary>
    private static async Task<bool> ReadExactAsync(NetworkStream stream, byte[] buffer, CancellationToken ct)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int bytesRead = await stream.ReadAsync(buffer, offset, buffer.Length - offset, ct);
            if (bytesRead == 0) return false; // clean disconnect
            offset += bytesRead;
        }
        return true;
    }

    /// <summary>
    /// Sends a length-prefixed response back to the client.
    /// </summary>
    private static async Task SendResponseAsync(NetworkStream stream, string response)
    {
        byte[] data   = Encoding.UTF8.GetBytes(response);
        byte[] header = BitConverter.GetBytes(data.Length);
        if (BitConverter.IsLittleEndian) Array.Reverse(header);

        await stream.WriteAsync(header);
        await stream.WriteAsync(data);
    }

    /// <summary>
    /// Builds the log file path for the current day.
    /// </summary>
    private static string GetLogFilePath()
    {
        string folderName = "logs";
        string fileName   = $"{DateTime.Now:yyyy-MM-dd}";
        return Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);
    }
}
