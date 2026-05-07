using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using EasyLog.Serializers;

namespace EasyLog.Loggers;

public abstract class AbstractDistantLogger
{
    private readonly string _type;
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
            using var client = new TcpClient(ServerName, ServerPort);
            using NetworkStream stream = client.GetStream();
            stream.Write(data, 0, data.Length);
        }
        catch (SocketException socketException)
        {
            Console.WriteLine("[-] Impossible to contact distant server for the centralisation of logs. " + socketException.Message);
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

        dictionaryToReturn.TryAdd("LogType", _type);

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
        return mac ?? "000000000000";
    }
}
