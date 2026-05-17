namespace EasyLog.Loggers;

public class JsonDistantLogger(string serverName, int serverPort) : AbstractDistantLogger(serverName, serverPort, "json")
{
}
