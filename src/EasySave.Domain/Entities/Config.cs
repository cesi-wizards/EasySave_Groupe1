namespace EasySave.Domain.Entities;

public class Config
{
    // Attributs
    public string SaveName { get; } = string.Empty;
    public string SourceFolder { get; } = string.Empty;
    public string TargetFolder { get; } = string.Empty;

    // Constructors
    public Config() { }
    public Config(string saveName, string sourceFolder, string targetFolder)
    {
        SaveName = saveName;
        SourceFolder = sourceFolder;
        TargetFolder = targetFolder;
    }
}
