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


    public void SendToRemoteServer(Dictionary<string, object> dictionaryContent)
    {
        try
        {
            string payload = FormatDictionary(dictionaryContent);
            byte[] data = Encoding.UTF8.GetBytes(payload);

            // Tcp connection and transfer to server
            using var client = new TcpClient(ServerName, ServerPort);
            using NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);

            client.Client.Shutdown(SocketShutdown.Send);

            // Treating response from server
            byte[] responseBuffer  = new byte[1024];
            int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);

            string response = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);
            if (response != "OK")
            {
                Debug.WriteLine($"[-] Server returned an error: {response}");
            }
        }
        catch (SocketException ex)
        {
            Debug.WriteLine("[-] Impossible to contact distant server for the centralisation of logs. " + ex.Message);
        }
    }

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
}
