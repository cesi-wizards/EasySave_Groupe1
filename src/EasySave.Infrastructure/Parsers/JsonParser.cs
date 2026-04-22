using System.Text.Json;

namespace EasySave.Infrastructure.Parsers;

public class JsonParser
{
   
    FileConfig Parse(string filePath)
    {
        if (!File.Exists(filePath))     // Check if the File already exists
        {
            throw new Exception();
        }

        string fileData = File.ReadAllText(filePath);
        var jsonConfigFile = JsonSerializer.Deserialize<FileConfig>(fileData);
        return jsonConfigFile;
    }
}
