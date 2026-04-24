using System.Diagnostics;

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

    protected abstract (List<string>, int, long) GetFilesToBackup(string sourcePath, string targetPath);

    public void Execute(string JobName, string sourcePath, string targetPath)
    {
        var (toBackup, count, size) = GetFilesToBackup(sourcePath, targetPath);
        int remainingCount = count; long remainingSize = size;

        foreach (string sourceFile in toBackup)
        {
            long fileSize = new FileInfo(sourceFile).Length;

            var targetFile = GetTargetFile(sourcePath, targetPath, sourceFile);

            Context CreateContext(TimeSpan transferTime)
            {
                return new Context(jobName: JobName, timestamp: new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(),
                    sourcePath: sourceFile, targetPath: targetFile, fileSize: fileSize, transferTime: transferTime,
                    totalCount: count, totalSize: size, remainingCount: remainingCount, remainingSize: remainingSize);
             }

            Context contextPreBackup = CreateContext(TimeSpan.Zero);
            Notify(contextPreBackup);

            TimeSpan transferTime = CopyFile(sourceFile, targetFile);
            remainingCount--; remainingSize -= fileSize;

            Context contextPostBackup = CreateContext(transferTime);
            Notify(contextPostBackup);
        }
    }
}
