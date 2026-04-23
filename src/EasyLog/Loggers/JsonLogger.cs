using System.Text.Json;
using EasyLog.Interfaces;

namespace EasyLog.Loggers;

public class JsonLogger : ILogger
{
    public string FilePath { get; }

    // Locker for multithreading
    private static readonly object _lock = new();

    public JsonLogger(string filePath)
    {
        FilePath = FilePathToJsonLinePath(filePath);
    }

    public void Write(Dictionary<string, object> dictionatyContent)
    {
        lock (_lock)
        {
            try
            {
                WriteJson(ContentToJson(dictionatyContent));
            }
            catch (Exception ex)
            {
                throw new IOException("Error, couldn't write within the file.", ex);
            }
        }
    }

    private void WriteJson(string jsonContent)
    {
        // Ensures the directory to write into exists
        EnsureDirectoryExists();

        // result : { "data": "..." , ... }
        // if file didn't existed it will also create it
        File.AppendAllText(FilePath, jsonContent + Environment.NewLine);
    }

    // ----------- Utilitairies

    /// <summary>
    /// Forces the file to have .json extention to write into
    /// </summary>
    /// <param name="filePath"></param>
    private string FilePathToJsonLinePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            filePath = "default_log.jsonl";
        }

        return Path.ChangeExtension(filePath, ".jsonl");
    }

    /// <summary>
    /// Serialises a dictionary
    /// </summary>
    /// <param name="dictionaryContent"></param>
    /// <returns></returns>
    private string ContentToJson(Dictionary<string, object> dictionaryContent)
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
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Impossible to serialize the content for the log in JSON.", ex);
        }
    }

    /// <summary>
    /// Write the whole chain of directories for the file to be saved in
    /// </summary>
    private void EnsureDirectoryExists()
    {
        try
        {
            string directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Permission denied to create the folder : {FilePath}", ex);
        }
        catch (PathTooLongException ex)
        {
            throw new IOException($"Filepath is too long for the file tree : {FilePath}", ex);
        }
    }
}
