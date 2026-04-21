using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Strategies;

namespace EasySave.Infrastructure.Factories;

public class DifferentialBackupFactory : AbstractBackupFactory
{
    public override BackupJob CreateJob(string jobName, string srcPath, string targetPath)
    {
        IBackupStrategy strategy = new DifferentialBackupStrategy();
        return CreateJobWithStrategy(jobName, srcPath, targetPath, strategy);
    }
}
