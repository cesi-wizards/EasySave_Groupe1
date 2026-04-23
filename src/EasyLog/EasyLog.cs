using EasyLog.Interfaces;
using EasyLog.Loggers;

namespace EasyLog;

public class EasyLog
{
    // Instance (singleton design pattern)
    private static EasyLog _instance;

    // Locker for multithreading
    private static readonly object _lock = new();

    private ILogger _logger;

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
            // Only create it if it wasn't already a JsonLogger
            if (_logger is not JsonLogger)
            {
                _logger = new JsonLogger(filePath);
            }
            else if (_logger.FilePath != filePath)
            {
                _logger = new JsonLogger(filePath);
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
            default:
            {
                CreateJsonLogger(filePath);
                break;
            }
        }
        _logger.Write(content);
    }
}
