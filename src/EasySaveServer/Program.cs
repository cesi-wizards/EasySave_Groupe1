using System.Net;
using System.Net.Sockets;
using System.Text;

class Program
{
    static async Task Main(string[] args)
    {
        int port = 11000;
        TcpListener listener = new TcpListener(IPAddress.Any, port);

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
        catch(Exception ex)
        {
            Console.WriteLine($"[Server Error]: {ex.Message}");
        }
    }

    private static async Task HandleClientAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (NetworkStream stream = client.GetStream())
            {

                // read incoming message
                byte[] buffer = new byte[8192]; // Stocks the incoming message
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.WriteLine($"[Log  @{DateTime.Now}] : {message}");

                    Dictionary<string, object> content = System.Text.Json.JsonSerializer
                                                             .Deserialize<Dictionary<string, object>>(message)
                                                         ?? new Dictionary<string, object> { { "raw", message } };
                    string logType = "json";

                    if (content.TryGetValue("LogType", out object? logTypeValue))
                    {
                        logType = logTypeValue?.ToString() ?? "json";
                    }

                    EasyLog.EasyLog.Instance.Write(GetLogFilePath(), content, logType);
                }
            }
        }
        catch (SocketException socketException)
        {
            Console.WriteLine($"[Socket error] : ex: {socketException.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERREUR]: {ex.Message}");
        }
    }

    private static string GetLogFilePath()
    {
        string folderName = "Logs";        string fileName = $"{DateTime.Now:yyyy-MM-dd}";
        string local = Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);
        return local;
    }
}
