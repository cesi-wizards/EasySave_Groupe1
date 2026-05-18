using EasySave.Domain.Entities;

namespace EasySave.Domain.Interfaces;

public interface IBackupFactory
{
    BackupJob CreateJob(string jobName, string srcPath, string targetPath, List<string> TypesToEncrypt, string encryptKey);
}