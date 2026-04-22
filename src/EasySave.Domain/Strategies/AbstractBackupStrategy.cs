using System.Diagnostics;

using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Strategies;

public abstract class AbstractBackupStrategy : IBackupStrategy, IPublisher
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
    public void ValidateSubscriber(ISubscriber subscriber)
    {
        if (subscriber == null)
            throw new ArgumentNullException(nameof(subscriber));
    }
    public void Attach(ISubscriber subscriber)
    {
        ValidateSubscriber(subscriber);
        if (!Subscribers.Contains(subscriber))
        {
            Subscribers.Add(subscriber);
        }
    }
    public void Detach(ISubscriber subscriber)
    {
        ValidateSubscriber(subscriber);
        Subscribers.Remove(subscriber);
    }

    public abstract void Execute(string JobName, string sourcePath, string targetPath);

    protected IEnumerable<string> GetFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return [];
        }
        return Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories); // recursive search acting as a generator
    }
    protected string GetTargetFile(string sourcePath, string targetPath, string sourceFile)
    {
        var relativePath = Path.GetRelativePath(sourcePath, sourceFile);
        var targetFile = Path.Combine(targetPath, relativePath);
        return targetFile;
    }
    protected TimeSpan CopyFile(string sourceFile, string targetFile)
    {
        var targetDirectory = Path.GetDirectoryName(targetFile);
        if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }
        var stopwatch = Stopwatch.StartNew();

        try
        {
            File.Copy(sourceFile, targetFile, overwrite: true);
        }
        catch
        {
            stopwatch.Stop();
            return TimeSpan.FromMilliseconds(-1); // return negative time if the copy operation fails
        }

        stopwatch.Stop(); return stopwatch.Elapsed; // get float milliseconds with TimeSpan with (float)duration.TotalMilliseconds;
    }
}
