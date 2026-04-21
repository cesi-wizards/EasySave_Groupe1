namespace EasySave.Infrastructure.Parsers;

public class JsonParser
{
    Config IConfigParser.Parse(string filePath)
    {
        if (!File.Exists(filePath))     // Check if the File already exists
        {
            return new Config();        // return a void Config object
        }

        string name = Path.GetFileName(filePath);
        string targetRepository = string.Empty;

        Config configFile = new Config(name, filePath, targetRepository);

        return configFile;
    }
}
