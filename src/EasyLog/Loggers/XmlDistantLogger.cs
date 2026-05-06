namespace EasyLog.Loggers;

public class XmlDistantLogger :  AbstractDistantLogger
{
    public XmlDistantLogger(string serverName, int serverPort) : base(serverName, serverPort, "xml") { }
}
