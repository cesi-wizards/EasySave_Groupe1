namespace EasyLog.Interfaces;

/*
* Logger interface
*/

public interface ILogger
{
    void Write(string filepath, string content);
}
