using EasyLog.Interfaces;
using EasyLog.Loggers;

namespace EasyLog;

public class EasyLog : IEasyLog
{
    // Instance (singleton design pattern)
    private static EasyLog? _instance;

    // Locker for multithreading
    private static readonly object _lock = new();

    private AbstractLogger? _localLogger;

    private AbstractDistantLogger? _distantLogger;

    /// <summary>
    /// Instance getter (singleton design pattern)
    /// </summary>
    public static EasyLog Instance
    {
        get
        {
            lock (_lock) // Threadsafe
            {
                _instance ??= new EasyLog();
                return _instance;
            }
        }
    }

    // Private constructor (singleton design pattern)
    private EasyLog(){}

    /// <summary>
    /// instantiate the logger as a JsonLogger
    /// </summary>
    /// <param name="filePath"></param>
    private void CreateJsonLogger(string filePath)
    {
        string targetPath = Path.ChangeExtension(filePath, ".jsonl");

        if (_localLogger is not JsonLogger ||_localLogger.FilePath != targetPath)
        {
            _localLogger = new JsonLogger(filePath);
        }
    }

    private void CreateXmlLogger(string filePath)
    {
        string targetPath = Path.ChangeExtension(filePath, ".xml");

        if (_localLogger is not XmlLogger || _localLogger.FilePath != targetPath)
        {
            _localLogger = new XmlLogger(filePath);
        }
    }

    private void CreateJsonDistantLogger(string serverName, int serverPort)
    {
        if (_distantLogger is not JsonDistantLogger || _distantLogger.ServerName != serverName || _distantLogger.ServerPort != serverPort)
        {
            _distantLogger = new JsonDistantLogger(serverName, serverPort);
        }
    }

    private void CreateXmlDistantLogger(string serverName, int serverPort)
    {
        if (_distantLogger is not XmlDistantLogger || _distantLogger.ServerName != serverName || _distantLogger.ServerPort != serverPort)
        {
            _distantLogger = new XmlDistantLogger(serverName, serverPort);
        }
    }

    /// <summary>
    /// Old method, kept for backward compatibility, redirects to the new WriteMethod
    /// </summary>
    public void Write(string filePath, Dictionary<string, object> content, string type)
    {
        Write(filePath, content, type, string.Empty, 0);
    }

    /// <summary>
    /// WriteMethod
    /// </summary>
    /// <param name="filePath"> Path of the file you want to save the logfile in (useless if isDistant is true) </param>
    /// <param name="content"> Dictionary containing the information to log </param>
    /// <param name="type"> type in which the log file will be written </param>
    /// <param name="serverName"> Defines the address to reach the server, leave blank to not use centralised login </param>
    /// <param name="serverPort"> Defines the port to communicate with the server </param>
    public void Write(string filePath, Dictionary<string, object> content, string type, string serverName = "", int serverPort = 0)
    {
        string format = (type ?? "json").ToLower();

        lock (_lock)
        {
            if (!string.IsNullOrWhiteSpace(serverName) && serverPort > 0)
            {
                switch (format)
                {
                    case "xml":
                        CreateXmlDistantLogger(serverName, serverPort);
                        break;
                    default:
                        CreateJsonDistantLogger(serverName, serverPort);
                        break;
                }
                _distantLogger.SendToRemoteServer(content);
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                switch (format)
                {
                    case "xml":
                        CreateXmlLogger(filePath);
                        break;
                    default:
                        CreateJsonLogger(filePath);
                        break;
                }
                _localLogger.Write(content);
            }

            if (_localLogger == null && _distantLogger == null)
                throw new InvalidOperationException("Logger not initialised, fatal error");
        }
    }
}
