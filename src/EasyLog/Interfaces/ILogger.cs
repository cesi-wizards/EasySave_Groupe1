namespace EasyLog.Interfaces;

/*
* Logger interface
*/

public interface ILogger
{
    string FilePath { get; }
    void Write(Dictionary<string, object> content);
}
