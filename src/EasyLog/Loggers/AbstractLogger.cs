namespace EasyLog.Loggers;

/*
* Logger interface
*/

public abstract class AbstractLogger
{
    public string FilePath { get; protected set; }

    protected AbstractLogger(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    public abstract void  Write(Dictionary<string, object> content);

    protected void EnsureDirectoryExists()
    {
        try
        {
            string? directory = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Permission denied when creating the folder: {FilePath}", ex);
        }
        catch (PathTooLongException ex)
        {
            throw new IOException($"File path is too long for the file tree: {FilePath}", ex);
        }
    }
}
