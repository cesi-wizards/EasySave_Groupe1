using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Strategies;

public abstract class AbstractBackupStrategy : IBackupStrategy
{
    public List<ISubscriber> Subscribers { get; set; } = [];
    public abstract void Execute(string sourcePath, string targetPath);
    public void Attach(ISubscriber subscriber)
    {
        // Implementation of attaching a subscriber
    }
    public void Detach(ISubscriber subscriber)
    {
        // Implementation of detaching a subscriber
    }
}
