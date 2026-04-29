using EasyLog.Loggers;

namespace EasyLog;

public class EasyLog
{
    // Instance (singleton design pattern)
    private static EasyLog? _instance;

    // Locker for multithreading
    private static readonly object _lock = new();

    private AbstractLogger? _abstractLogger;

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
        lock (_lock)
        {
            string targetPath = Path.ChangeExtension(filePath, ".jsonl");

            if (_abstractLogger is not JsonLogger || _abstractLogger.FilePath != targetPath)
            {
                _abstractLogger = new JsonLogger(filePath);
            }
        }
    }

    private void CreateXmlLogger(string filePath)
    {
        lock (_lock)
        {
            string targetPath = Path.ChangeExtension(filePath, ".xml");

            if (_abstractLogger is not XmlLogger || _abstractLogger.FilePath != targetPath)
            {
                _abstractLogger = new XmlLogger(filePath);
            }
        }
    }

    /// <summary>
    /// Default easylog file
    /// </summary>
    /// <param name="filePath">Path of the file to save into</param>
    /// <param name="content">Content to save</param>
    /// <param name="type"> The type of logger to use ("Json", "Xml", ...)</param>
    public void Write(string filePath, Dictionary<string, object> content, string type)
    {
        string format = (type ?? "json").ToLower();

        switch (format)
        {
            case ("json") :
            {
                CreateJsonLogger(filePath);
                break;
            }
            case ("xml"):
            {
                CreateXmlLogger(filePath);
                break;
            }
            default:
            {
                CreateJsonLogger(filePath);
                break;
            }
        }

        if (_abstractLogger is null)
        {
            throw new InvalidOperationException("Logger not initialized.");
        }
        _abstractLogger.Write(content);
    }
}
