namespace EasyLog.Interfaces;

/*
* Logger interface
*/

public interface ILogger
{
    void Write(Dictionary<string, object> content);
}
