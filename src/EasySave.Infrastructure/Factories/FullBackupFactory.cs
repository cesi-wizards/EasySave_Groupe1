using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Strategies;

namespace EasySave.Infrastructure.Factories;

public class FullBackupFactory : AbstractBackupFactory
{
    public override BackupJob CreateJob(string jobName, string srcPath, string targetPath)
    {
        IBackupStrategy strategy = new FullBackupStrategy();
        return CreateJobWithStrategy(jobName, srcPath, targetPath, strategy);
    }
}
