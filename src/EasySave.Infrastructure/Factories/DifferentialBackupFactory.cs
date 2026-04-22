using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Strategies;

namespace EasySave.Infrastructure.Factories;

public class DifferentialBackupFactory(List<ISubscriber> subscribers) : AbstractBackupFactory(subscribers)
{
    public override BackupJob CreateJob(string jobName, string srcPath, string targetPath)
    {
        AbstractBackupStrategy strategy = new DifferentialBackupStrategy();
        return CreateJobWithStrategy(jobName, srcPath, targetPath, strategy);
    }
}
