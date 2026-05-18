using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Strategies;

namespace EasySave.Infrastructure.Factories;

public abstract class AbstractBackupFactory(List<ISubscriber> subscribers, ISoftwareDetector? softwareDetector = null) : IBackupFactory
{
    private List<ISubscriber> GlobalSubscribers { get; init; } = subscribers;
    protected ISoftwareDetector? SoftwareDetector { get; } = softwareDetector;

    public abstract BackupJob CreateJob(string jobName, string srcPath, string targetPath, List<string> TypesToEncrypt, string encryptKey);

    protected void WireSubscribers(IPublisher publisher)
    {
        foreach (ISubscriber subscriber in GlobalSubscribers)
        {
            publisher.Attach(subscriber);
        }
    }

    protected BackupJob CreateJobWithStrategy(string jobName, string sourcePath, string targetPath,
        AbstractBackupStrategy strategy, List<string> TypesToEncrypt, string encryptKey)
    {
        WireSubscribers(strategy);
        return new BackupJob(jobName, sourcePath, targetPath, strategy, TypesToEncrypt, encryptKey);
    }
}
