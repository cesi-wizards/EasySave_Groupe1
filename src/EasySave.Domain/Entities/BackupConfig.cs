namespace EasySave.Domain.Entities;

public class BackupConfig
{
    public required string Name { get; set; }
    public required string SourcePath { get; set; }
    public required string TargetPath { get; set; }
    public required BackupType Type { get; set; }
}
