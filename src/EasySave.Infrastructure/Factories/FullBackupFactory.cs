using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Strategies;

namespace EasySave.Infrastructure.Factories;

public class FullBackupFactory(List<ISubscriber> subscribers) : AbstractBackupFactory(subscribers)
{
    public override BackupJob CreateJob(string jobName, string sourcePath, string targetPath, List<string> encryptTypes, string encryptKey)
    {
        AbstractBackupStrategy strategy = new FullBackupStrategy();
        return CreateJobWithStrategy(jobName, sourcePath, targetPath, strategy, encryptTypes, encryptKey);
    }
}
