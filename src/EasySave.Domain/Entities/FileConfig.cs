namespace EasySave.Domain.Entities;

public class FileConfig(string language)
{
    public string Language { get; init; } = language;
    public List<BackupConfig> Jobs { get; init; } = [];

}
