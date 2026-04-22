namespace EasySave.Domain.Entities;

public class BackupConfig(string Name, string SourcePath, string TargetPath, BackupType Type)
{
    public required string Name { get; init; } = Name;
    public required string SourcePath { get; init; } = SourcePath;
    public required string TargetPath { get; init; } = TargetPath;
    public required BackupType Type { get; init; } = Type;
}
