namespace EasySave.Domain.Entities;

public class FileConfig(string language, string logFileType)
{
    public string Language { get; init; } = language;

    public string LogFileType { get; init; } = logFileType;
    public List<BackupConfig> Jobs { get; init; } = [];
    public List<string> BusinessSoftwares {  get; init; } = [];

}
