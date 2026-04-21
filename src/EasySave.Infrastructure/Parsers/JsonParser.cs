using System.Text.Json;

namespace EasySave.Infrastructure.Parsers;

public class JsonParser
{
   
    Config Parse(string filePath)
    {
        if (!File.Exists(filePath))     // Check if the File already exists
        {
            return new Config();        // return a void Config object
        }

        string fileData = File.ReadAllText(filePath);

        var json = JsonSerializer.Deserialize<Config>(fileData);

        string name = json.Name;
        string sourceFolder = json.SourceFolder;
        string targetFolder = json.TargetFolder;

        Config configFile = new Config(name, sourceFolder, targetFolder);

        return configFile;
    }
}
