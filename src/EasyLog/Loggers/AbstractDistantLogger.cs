using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace EasyLog.Loggers;

public abstract class AbstractDistantLogger
{
    private string _type;

    public string ServerName { get; protected set; }
    public int ServerPort { get; protected set; }

    protected AbstractDistantLogger(string serverName, int serverPort, string type)
    {
        ServerName = serverName ?? throw new ArgumentNullException(nameof(serverName));
        ServerPort = serverPort;
        _type = type;
    }

    public void SendToRemoteServer(Dictionary<string, object> dictionaryContent)
    {
        try
        {
            string payload = FormatDictionary(dictionaryContent);
            byte[] data = Encoding.UTF8.GetBytes(payload);

            // Tcp connection and transfer to server
            using TcpClient client = new TcpClient(ServerName, ServerPort);
            using NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }
        catch (SocketException socketException)
        {
            Console.WriteLine("[-] Impossible to contact distant server for the centralisation of logs.");
        }
    }

    private string FormatDictionary(Dictionary<string, object> dictionaryContent)
    {
        try
        {
            dictionaryContent = AddLogTypeToDictionary(dictionaryContent);
            // get source mac address
            dictionaryContent = AddMacAddressToDictionary(dictionaryContent);
            string jsonLine = ContentToJson(dictionaryContent);

            return jsonLine;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    private Dictionary<string, object> AddLogTypeToDictionary(Dictionary<string, object> baseDictionary)
    {
        Dictionary<string, object> dictionaryToReturn = new Dictionary<string, object>(baseDictionary);

        if (!dictionaryToReturn.ContainsKey("LogType"))
        {

            dictionaryToReturn.Add("LogType", _type);
        }
        return dictionaryToReturn;
    }

    protected Dictionary<string, object> AddMacAddressToDictionary(Dictionary<string, object> baseDictionary)
    {
        Dictionary<string, object> dictionaryToReturn = new Dictionary<string, object>(baseDictionary);

        if (!dictionaryToReturn.ContainsKey("SourceMacAddress"))
        {
            dictionaryToReturn.Add("SourceMacAddress", GetMacAddress());
        }
        return dictionaryToReturn;
    }

    private string GetMacAddress()
    {
        var mac = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault();
        return mac ?? "000000000000";
    }

    private string ContentToJson(Dictionary<string, object> dictionaryContent)
    {
        // If the dictionary is null or empty, writes an empty log
        if (dictionaryContent == null || dictionaryContent.Count == 0)
        {
            return "{}";
        }

        try
        {
            // options for the serialization
            var options = new JsonSerializerOptions
            {
                WriteIndented = false, // writes all un a single line
                PropertyNamingPolicy = null // keep the name of the keys
            };

            return JsonSerializer.Serialize(dictionaryContent, options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Impossible to serialize the content for the log in JSON.", ex);
        }
    }
}
