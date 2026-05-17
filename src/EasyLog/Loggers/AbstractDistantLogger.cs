using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using EasyLog.Serializers;

namespace EasyLog.Loggers;

public abstract class AbstractDistantLogger(string serverName, int serverPort, string type)
{
    public string ServerName { get; init; } = serverName;
    public int ServerPort { get; init; } = serverPort;

    private TcpClient? _client;
    private NetworkStream? _stream;
    private readonly SemaphoreSlim _semaphore = new(1,1);
    private const int _connectionTimeoutMs = 5000;
    // ------------------------------
    // Connection to the server
    // ------------------------------

    public async Task SendToRemoteServerAsync(Dictionary<string, object> dictionaryContent)
    {
        await _semaphore.WaitAsync();
        try
        {
            await EnsureConnectedAsync();

            string payload = FormatDictionary(dictionaryContent);
            byte[] data    = Encoding.UTF8.GetBytes(payload);

            // --- Protocol: write 4-byte big-endian length header then payload
            byte[] header = BitConverter.GetBytes(data.Length);
            if (BitConverter.IsLittleEndian) Array.Reverse(header); // enforce big-endian

            await _stream!.WriteAsync(header);
            await _stream!.WriteAsync(data);

            // --- Read response
            string response = await ReadResponseAsync();
            if (response != "OK")
                Debug.WriteLine($"[-] Server returned an error: {response}");
        }
        catch (SocketException ex)
        {
            Debug.WriteLine("[-] Impossible to contact distant server. " + ex.Message);
            ResetConnection();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("[-] Unexpected error while sending log. " + ex.Message);
            ResetConnection();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // ------------------------------
    // Utility for connection
    // ------------------------------

    private async Task EnsureConnectedAsync()
    {
        if (_client is { Connected: true } && _stream != null) return;
        ResetConnection();

        _client = new TcpClient();

        using var cts = new CancellationTokenSource(_connectionTimeoutMs);
        try
        {
            await _client.ConnectAsync(ServerName, ServerPort, cts.Token);
        }
        catch (OperationCanceledException)
        {
            ResetConnection();
            throw new TimeoutException($"Connection to {ServerName}:{ServerPort} timed out after {_connectionTimeoutMs}ms.");
        }

        _stream = _client.GetStream();
    }

    private async Task<string> ReadResponseAsync()
    {
        byte[] headerBuffer = new byte[4];
        await ReadExactAsync(_stream!, headerBuffer);
        if (BitConverter.IsLittleEndian) Array.Reverse(headerBuffer);
        int length = BitConverter.ToInt32(headerBuffer);

        // Read exactly <length> bytes
        byte[] responseBuffer = new byte[length];
        await ReadExactAsync(_stream!, responseBuffer);

        return Encoding.UTF8.GetString(responseBuffer);
    }

    private static async Task ReadExactAsync(NetworkStream stream, byte[] buffer)
    {
        int offset = 0;
        while (offset < buffer.Length)
        {
            int bytesRead = await stream.ReadAsync(buffer, offset, buffer.Length - offset);
            if (bytesRead == 0)
                throw new IOException("Connection closed by server before all bytes were received.");
            offset += bytesRead;
        }
    }

    private void ResetConnection()
    {
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
    }

    // ------------------------------
    // Formating
    // ------------------------------

    private string FormatDictionary(Dictionary<string, object> dictionaryContent)
    {
        try
        {
            dictionaryContent = AddLogTypeToDictionary(dictionaryContent);
            // get source mac address
            dictionaryContent = AddMacAddressToDictionary(dictionaryContent);
            string jsonLine = JsonLogSerializer.DictionaryToJson(dictionaryContent);

            return jsonLine;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private Dictionary<string, object> AddLogTypeToDictionary(Dictionary<string, object> baseDictionary)
    {
        var dictionaryToReturn = new Dictionary<string, object>(baseDictionary);

        dictionaryToReturn.TryAdd("LogType", type);

        return dictionaryToReturn;
    }

    private Dictionary<string, object> AddMacAddressToDictionary(Dictionary<string, object> baseDictionary)
    {
        var dictionaryToReturn = new Dictionary<string, object>(baseDictionary);

        dictionaryToReturn.TryAdd("SourceMacAddress", GetMacAddress());

        return dictionaryToReturn;
    }

    private string GetMacAddress()
    {
        string? mac = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault();
        if (string.IsNullOrEmpty(mac))
        {
            return "00:00:00:00:00:00";
        }
        return Regex.Replace(mac, ".{2}", "$0:").TrimEnd(':');
    }

    // ----------
    // IAsync Disposable
    // ----------
    public async ValueTask DisposeAsync()
    {
        if (_stream != null) await _stream.DisposeAsync();
        _client?.Dispose();
    }
}
