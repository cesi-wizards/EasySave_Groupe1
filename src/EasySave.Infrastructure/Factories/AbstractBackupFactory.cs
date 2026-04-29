using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Strategies;
using EasySave.Infrastructure.Factories.Interfaces;

namespace EasySave.Infrastructure.Factories;

public abstract class AbstractBackupFactory(List<ISubscriber> subscribers) : IBackupFactory
{
    private List<ISubscriber> GlobalSubscribers { get; init; } = subscribers;

    public abstract BackupJob CreateJob(string jobName, string srcPath, string targetPath, List<string> encryptTypes, string encryptKey);

    protected void WireSubscribers(IPublisher publisher)
    {
        foreach (ISubscriber subscriber in GlobalSubscribers)
        {
            publisher.Attach(subscriber);
        }
    }

    protected BackupJob CreateJobWithStrategy(string jobName, string sourcePath, string targetPath,
        AbstractBackupStrategy strategy, List<string> encryptTypes, string encryptKey)
    {
        WireSubscribers(strategy);
        return new BackupJob(jobName, sourcePath, targetPath, strategy, encryptTypes, encryptKey);
    }
}
