using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Strategies;

public abstract class AbstractBackupStrategy : IBackupStrategy, IPublisher
{
    public List<ISubscriber> Subscribers { get; set; } = [];
    protected void Notify(Context context) // protected to prevent external classes from triggering notifications directly
    {
        foreach (var subscriber in Subscribers)
        {
            subscriber.Update(context);
        }
    }
    public void Attach(ISubscriber subscriber)
    {
        if (subscriber == null)
            throw new ArgumentNullException(nameof(subscriber));
        if (!Subscribers.Contains(subscriber))
        {
            Subscribers.Add(subscriber);
        }
    }
    public void Detach(ISubscriber subscriber)
    {
        if (subscriber == null)
            throw new ArgumentNullException(nameof(subscriber));
        Subscribers.Remove(subscriber);
    }
    public abstract void Execute(string sourcePath, string targetPath);
}
