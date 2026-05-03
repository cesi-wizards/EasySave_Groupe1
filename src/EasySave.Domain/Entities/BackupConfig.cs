namespace EasySave.Domain.Entities;

public class BackupConfig(string name, string sourcePath, string targetPath, BackupType type, string logFileType, List<string> TypesToEncrypt, string encryptKey)
{
    public required string Name { get; init; } = name;
    public required string SourcePath { get; init; } = sourcePath;
    public required string TargetPath { get; init; } = targetPath;
    public required BackupType Type { get; init; } = type;
    public string LogFileType { get; set; } = logFileType;
    public List<string> TypesToEncrypt { get; init; } = TypesToEncrypt;
    public string EncryptKey { get; init; } = encryptKey;
}
