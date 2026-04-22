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
    private void CreateJsonLogger()
    {
        // Only create it if it wasn't already a JsonLogger
        if (_logger is not JsonLogger)
        {
            _logger = new JsonLogger();
        }
    }

    /// <summary>
    /// Default easylog file
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="content"></param>
    public void LogJson(string jsonFilePath, string content)
    {
        CreateJsonLogger();
        _logger.Write(jsonFilePath, content);
    }
}
