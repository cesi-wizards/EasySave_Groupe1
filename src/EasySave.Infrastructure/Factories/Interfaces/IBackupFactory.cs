using EasySave.Domain.Entities;

namespace EasySave.Infrastructure.Factories.Interfaces;

public interface IBackupFactory
{
    public BackupJob CreateJob(string jobName, string srcPath, string targetPath);
}
