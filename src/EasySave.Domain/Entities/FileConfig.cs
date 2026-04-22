namespace EasySave.Domain.Entities;

public class FileConfig(string saveName, string sourceFolder, string targetFolder, string copyType)
{
    // ajouter required et const -> voir comment les ajouter

    // Attributes
    public string SaveName { get; init;  } = saveName;
    public string SourceFolder { get; init; } = sourceFolder;
    public string TargetFolder { get; init;  } = targetFolder;
    public string CopyType { get; init; } = copyType;   // full or differential
}
