namespace EasyLog.Loggers;

public class JsonDistantLogger : AbstractDistantLogger
{
    public JsonDistantLogger(string serverName, int serverPort) : base(serverName, serverPort, "json") { }
}
