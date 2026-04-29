using System.Text.Json;
using EasySave.Domain.Entities;

namespace EasySave.Infrastructure.Parsers;

public class JsonParser
{

    public FileConfig Parse(string filePath)
    {
        if (!File.Exists(filePath))     // Check if the File already exists
        {
            throw new Exception();
        }

        string fileData = File.ReadAllText(filePath);
        var jsonConfigFile = JsonSerializer.Deserialize<FileConfig>(fileData)
            ?? throw new InvalidOperationException("Failed to deserialize config file");
        return jsonConfigFile;
    }
}
