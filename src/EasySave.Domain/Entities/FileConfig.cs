namespace EasySave.Domain.Entities;

public class FileConfig(string logFileType, string logEmplacement = "local")
{
    public string LogFileType { get; init; } = logFileType;
    /// <summary>"local", "server" or "both"</summary>
    public string LogEmplacement { get; init; } = logEmplacement;
    public List<BackupConfig> Jobs { get; init; } = [];
    public List<string> BusinessSoftwares { get; init; } = [];
    public List<string> PriorityExtensions { get; init; } = [];
    public long LargeFileSizeThresholdKb { get; init; } = 0;
}
