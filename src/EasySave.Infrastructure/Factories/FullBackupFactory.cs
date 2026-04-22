using EasySave.Domain.Entities;
using EasySave.Domain.Strategies;

namespace EasySave.Infrastructure.Factories;

public class FullBackupFactory : AbstractBackupFactory
{
    public override BackupJob CreateJob(string jobName, string srcPath, string targetPath)
    {
        AbstractBackupStrategy strategy = new FullBackupStrategy();
        return CreateJobWithStrategy(jobName, srcPath, targetPath, strategy);
    }
}
