namespace EasyLog.Interfaces;

/*
* Logger interface
*/

public abstract class AbstractLogger
{
    public string FilePath { get; protected set; }
    public abstract void  Write(Dictionary<string, object> content);

    protected void EnsureDirectoryExists()
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
