namespace EasySave.Domain.Entities;

public class BackupConfig(string name, string sourcePath, string targetPath, BackupType type, string logFileType, List<string> TypesToEncrypt, string encryptKey)
{
    public string Name { get; init; } = name;
    public string SourcePath { get; init; } = sourcePath;
    public string TargetPath { get; init; } = targetPath;
    public BackupType Type { get; init; } = type;
    public string LogFileType { get; set; } = logFileType;
    public List<string> TypesToEncrypt { get; init; } = TypesToEncrypt;
    public string EncryptKey { get; init; } = encryptKey;
}
