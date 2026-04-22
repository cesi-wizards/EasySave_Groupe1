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

    protected IEnumerable<string> GetFiles(string sourcePath)
    {
        if (!Directory.Exists(sourcePath))
        {
            return [];
        }
        return Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories); // recursive search acting as a generator
    }
    protected void CopyFile(string sourceFile, string targetFile)
    {
        var targetDirectory = Path.GetDirectoryName(targetFile);
        if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
        File.Copy(sourceFile, targetFile, overwrite: true);
    }
}
