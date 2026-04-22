using System.Text.Json;
using EasyLog.Interfaces;

namespace EasyLog.Loggers;

public class JsonLogger : ILogger
{
    private string _jsonFilePath = string.Empty;

    // name of the mutex (for multiprocess safety)
    private const string _mutexName = "Global\\EasySave_Log_Mutex";

    /// <summary>
    /// Implementation of the Write method for registering Json
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="content"></param>
    public void Write(string jsonFilePath, string content)
    {
        // null/empty safe
        if (!string.IsNullOrEmpty(jsonFilePath) && !string.IsNullOrEmpty(content))
        {
            if (!_jsonFilePath.Equals(jsonFilePath, StringComparison.OrdinalIgnoreCase))
            {
                _jsonFilePath = jsonFilePath;
            }

            // use/ create the global mutex
            using var mutex = new Mutex(false, _mutexName);
            bool hasHandle = false;

            try
            {
                hasHandle = mutex.WaitOne(TimeSpan.FromSeconds(5));
                // waiting for the mitex to get freed (5 seconds of timeout)
                if (hasHandle)
                {
                    EnsureDirectoryExists();
                    FilePathToJsonPath();
                    string jsonContent = ContentToJSON(content);

                    // check if file existed and was empty
                    bool fileExists = File.Exists(_jsonFilePath) && new FileInfo(_jsonFilePath).Length > 0;

                    if (!fileExists)
                    {
                        WriteJsonInNewFile(jsonContent);
                    }
                    else
                    {
                        WriteJsonInExistingFile(jsonContent);
                    }
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
    }

    /// <summary>
    /// Writes the Json in a new file
    /// </summary>
    /// <param name="jsonContent"></param>
    private void WriteJsonInNewFile(string jsonContent)
    {
        // result : [ { "data": "..." , ... } ]
        File.WriteAllText(_jsonFilePath, "[" + jsonContent + "]");
    }

    /// <summary>
    /// Writes the Json in an existing file
    /// </summary>
    /// <param name="jsonContent"></param>
    private void WriteJsonInExistingFile(string jsonContent)
    {
        // 1. On ouvre le fichier en mode Lecture/Écriture
        using (var fs = new FileStream(_jsonFilePath, FileMode.Open, FileAccess.ReadWrite))
        {
            if (fs.Length > 1)
            {
                fs.Seek(-1, SeekOrigin.End);

                // overwrites the "]"  
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(Environment.NewLine + "," + jsonContent + "]");
                }
            }
            else
            {
                // if file is emtpy or corrupted
                WriteJsonInNewFile(jsonContent);
            }
        }
    }

    // ----------- Utilitairies

    /// <summary>
    /// Forces the file to have .json extention to write into
    /// </summary>
    private void FilePathToJsonPath()
    {
        if (string.IsNullOrWhiteSpace(_jsonFilePath))
        {
            _jsonFilePath = "default_log.json";
        }

        _jsonFilePath =  Path.ChangeExtension(_jsonFilePath, ".json");
    }

    /// <summary>
    /// Check if the content was already serialised, else, serialise it
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    private string ContentToJSON(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return "{}";

        content = content.Trim();

        // 1. Check rapide : un JSON commence souvent par { ou [
        if ((content.StartsWith("{") && content.EndsWith("}")) ||
            (content.StartsWith("[") && content.EndsWith("]")))
        {
            try
            {
                // Try to parse to confirm it's json
                using (JsonDocument.Parse(content))
                {
                    return content; // C'est du JSON valide, on ne touche à rien
                }
            }
            catch (JsonException)
            {
            }
        }

        // If we get here, the string wasn't serialazed yet
        var logObject = new
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            content
        };

        return JsonSerializer.Serialize(logObject);
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
