using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using EasyLog.Interfaces;

namespace EasyLog.Loggers;

public class JsonLogger : ILogger
{
    // name of the mutex (for multiprocess safety)
    private const string _mutexName = "Global\\EasySave_Log_Mutex";

    /// <summary>
    /// Implementation of the Write method for registering Json
    /// </summary>
    /// <param name="filepath"></param>
    /// <param name="content"></param>
    public void Write(string filepath, string content)
    {
        // null/empty safe
        if (!string.IsNullOrEmpty(filepath) && !string.IsNullOrEmpty(content))
        {
            // use/ create the global mutex
            using var mutex = new Mutex(false, _mutexName);
            try
            {
                // waiting for the mitex to get freed (5 seconds of timeout)
                if (mutex.WaitOne(TimeSpan.FromSeconds(5)))
                {
                    EnsureDirectoryExists(filepath);
                    string jsonFilePath = FilePathToJsonPath(filepath);
                    string jsonContent = ContentToJSON(content);

                    // check if file existed and was empty
                    bool fileExists = File.Exists(jsonFilePath) && new FileInfo(jsonFilePath).Length > 0;

                    if (!fileExists)
                    {
                        // new file
                        File.WriteAllText(jsonFilePath, jsonContent + Environment.NewLine);
                    }
                    else
                    {
                        // existing file
                        File.AppendAllText(jsonFilePath, "," + jsonContent + Environment.NewLine);
                    }
                }
            }
            finally
            {
                // release the mutex for the other instances to use it
                mutex.ReleaseMutex();
            }
        }
    }

    // ----------- Utilitairies

    /// <summary>
    /// Forces the file to have .json extention to write into
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private string FilePathToJsonPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return "default_log.json";

        return Path.ChangeExtension(filePath, ".json");
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
    /// <param name="filepath"></param>
    private void EnsureDirectoryExists(string filepath)
    {
        string directory = Path.GetDirectoryName(filepath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
