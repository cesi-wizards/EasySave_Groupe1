namespace EasyLog.Interfaces;

public interface IEasyLog
{
    void Write(string filePath, Dictionary<string, object> content, string type);
}
