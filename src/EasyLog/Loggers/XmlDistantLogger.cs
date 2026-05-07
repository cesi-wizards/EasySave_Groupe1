namespace EasyLog.Loggers;

public class XmlDistantLogger(string serverName, int serverPort) :  AbstractDistantLogger (serverName, serverPort, "xml")
{
}
