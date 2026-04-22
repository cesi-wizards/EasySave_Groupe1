using System.Text.Json;
using EasyLog.Interfaces;

namespace EasyLog.Loggers;

public class JsonLogger : ILogger
{
    private string _jsonFilePath = string.Empty;

    // name of the mutex (for multiprocess safety)
    private const string _mutexName = "Global\\EasySave_Log_Mutex";

    /// <summary>
    /// Constructor initiation the file path
    /// </summary>
    /// <param name="jsonFilePath"></param>
    public JsonLogger(string jsonFilePath)
    {

        _jsonFilePath = FilePathToJsonPath(jsonFilePath);
    }

    /// <summary>
    /// Implementation of the Write method for registering Json
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="content"></param>
    public void Write(Dictionary<string, object> dictionatyContent)
    {
            // use/ create the global mutex
            using var mutex = new Mutex(false, _mutexName);
            bool hasHandle = false;

        try
        {
            hasHandle = mutex.WaitOne(TimeSpan.FromSeconds(5));
            // waiting for the mitex to get freed (5 seconds of timeout)
            if (hasHandle)
            {
                string jsonContent = ContentToJSON(dictionatyContent);
                WriteJson(jsonContent);
            }
        }
        finally
        {
            // release the mutex for the other instances to use it
            if (hasHandle)
            {
                mutex.ReleaseMutex();
            }
        }
    }

    /// <summary>
    /// Writes the Json
    /// </summary>
    /// <param name="jsonContent"></param>
    private void WriteJson(string jsonContent)
    {
        // Ensures the directory to write into exists
        EnsureDirectoryExists(); 

        // result : { "data": "..." , ... }
        // if file didn't existed it will also create it
        File.AppendAllText(_jsonFilePath, jsonContent + Environment.NewLine);
    }

    // ----------- Utilitairies

    /// <summary>
    /// Forces the file to have .json extention to write into
    /// </summary>
    private string FilePathToJsonPath(string jsonFilePath)
    {
        if (string.IsNullOrWhiteSpace(jsonFilePath))
        {
            jsonFilePath = "default_log.json";
        }

        return Path.ChangeExtension(jsonFilePath, ".json");
    }

    /// <summary>
    /// Check if the content was already serialised, else, serialise it
    /// </summary>
    /// <param name="dictionaryContent"></param>
    /// <returns></returns>
    private string ContentToJSON(Dictionary<string, object> dictionaryContent)
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
        catch (Exception)
        {
            // In case of an exception
            return "{}";
        }
    }

    /// <summary>
    /// Write the whole chain of directories for the file to be saved in
    /// </summary>
    private void EnsureDirectoryExists()
    {
        string directory = Path.GetDirectoryName(_jsonFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
