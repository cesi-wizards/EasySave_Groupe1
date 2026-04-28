using System.Text.Json;

namespace EasyLog.Loggers;

public class JsonLogger : AbstractLogger
{
    // Locker for multithreading
    private static readonly object _lock = new();

    public JsonLogger(string filePath) : base(FilePathToJsonLinePath(filePath)) {}

    public override void Write(Dictionary<string, object> dictionaryContent)
    {
        lock (_lock)
        {
            try
            {
                WriteJson(ContentToJson(dictionaryContent));
            }
            catch (Exception ex)
            {
                throw new IOException("Failed to write to the file.", ex);
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

    // ----------- Utilities

    /// <summary>
    /// Forces the file to have .jsonl extention to write into
    /// </summary>
    /// <param name="filePath"></param>
    private static string FilePathToJsonLinePath(string filePath)
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
}
