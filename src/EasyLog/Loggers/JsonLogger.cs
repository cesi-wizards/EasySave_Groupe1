using EasyLog.Serializers;

namespace EasyLog.Loggers;

public class JsonLogger(string filePath) : AbstractLogger(FilePathToJsonLinePath(filePath))
{
    // Locker for multithreading
    private static readonly object _lock = new();

    public override void Write(Dictionary<string, object> dictionaryContent)
    {
        lock (_lock)
        {
            try
            {
                WriteJson(JsonLogSerializer.DictionaryToJson(dictionaryContent));
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

        // if file didn't exist it will also create it
        File.AppendAllText(FilePath, jsonContent + Environment.NewLine);
    }

    // ----------- Utilities

    /// <summary>
    /// Forces the file to have .jsonl extension to write into
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
}
