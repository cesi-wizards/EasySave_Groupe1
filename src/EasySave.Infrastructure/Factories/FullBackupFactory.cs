using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Strategies;

namespace EasySave.Infrastructure.Factories;

public class FullBackupFactory(List<ISubscriber> subscribers) : AbstractBackupFactory(subscribers)
{
    public override BackupJob CreateJob(string jobName, string srcPath, string targetPath)
    {
        AbstractBackupStrategy strategy = new FullBackupStrategy();
        return CreateJobWithStrategy(jobName, srcPath, targetPath, strategy);
    }
}
