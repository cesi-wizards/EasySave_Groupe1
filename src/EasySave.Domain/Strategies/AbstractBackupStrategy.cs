using System.Diagnostics;

using EasySave.Domain.Interfaces;
using EasySave.Domain.Events;

namespace EasySave.Domain.Strategies;

public abstract class AbstractBackupStrategy : IBackupStrategy, IPublisher
{
    private readonly IEncryptionService _encryptionService;
    private readonly ISoftwareDetector? _softwareDetector;

    protected AbstractBackupStrategy(IEncryptionService encryptionService, ISoftwareDetector? softwareDetector = null)
    {
        _encryptionService = encryptionService;
        _softwareDetector = softwareDetector;
    }

    // observer pattern code

    public List<ISubscriber> Subscribers { get; set; } = [];

    protected void Notify(IBackupEvent e) // the protected scope prevents external classes from triggering notifications directly
    {
        foreach (var subscriber in Subscribers)
        {
            subscriber.Update(e);
        }
    }

    private void ValidateSubscriber(ISubscriber subscriber)
    {
        if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
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

    // boilerplate code for concrete strategies

    protected IEnumerable<string> GetAllFiles(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return [];
        }
        return Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories); // recursive search
    }

    protected string GetTargetFile(string sourcePath, string targetPath, string sourceFile)
    {
        var relativePath = Path.GetRelativePath(sourcePath, sourceFile);
        var targetFile = Path.Combine(targetPath, relativePath);
        return targetFile;
    }

    protected abstract (List<string> files, int totalCount, long totalSize) GetBackupFiles(string sourcePath, string targetPath);

    // boilerplate code for execute() method

    private bool IsSoftwareRunning(string jobName)
    {
        if (_softwareDetector?.IsSoftwareRunning() == true)
        {
            Notify(new BackupInterrupted(
                new EventMetadata { JobName = jobName },
                "Blocking business software is running"
            ));
            return true;
        }
        return false;
    }

    private TimeSpan CopyFile(string sourceFile, string targetFile)
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
        finally
        {
            stopwatch.Stop();
        }
         return stopwatch.Elapsed; // get float milliseconds with TimeSpan with (float)duration.TotalMilliseconds;
    }

    // main strategy execution method

    public void Execute(string jobName, string sourcePath, string targetPath, List<string> typesToEncrypt, string encryptKey, ManualResetEvent pauseEvent, ITransferGate gate)
    {
        if (IsSoftwareRunning(jobName)) return;

        var (toBackup, count, size) = GetBackupFiles(sourcePath, targetPath);

        if (gate != null)
        {
            int priorityCount = toBackup.Count(f => gate.IsPriority(Path.GetExtension(f)));
            gate.RegisterPriorityFiles(priorityCount);
            if (priorityCount > 0)
                toBackup = [.. toBackup.OrderByDescending(f => gate.IsPriority(Path.GetExtension(f)))];
        }

        int remainingCount = count;
        long remainingSize = size;
        int failedCount = 0;
        bool interrupted = false;

        foreach (string sourceFile in toBackup)
        {
            pauseEvent.WaitOne();

            if (IsSoftwareRunning(jobName)) { interrupted = true; break; }

            long fileSize = new FileInfo(sourceFile).Length;
            string extension = Path.GetExtension(sourceFile);
            var targetFile = GetTargetFile(sourcePath, targetPath, sourceFile);

            var fileInfo = new BackupFileInfo(sourceFile, fileSize, targetFile);
            var beforeTransfer = new BackupProgress(count, remainingCount, size, remainingSize);
            Notify(new FileTransferReady(
                    new EventMetadata { JobName = jobName },
                    fileInfo,
                    beforeTransfer
                ));

            if (IsSoftwareRunning(jobName)) { interrupted = true; break; }

            TimeSpan transferTime = TimeSpan.Zero;

            gate?.Acquire(extension, fileSize);
            try
            {
                transferTime = CopyFile(sourceFile, targetFile);
            }
            catch (Exception e)
            {
                failedCount++;
                Notify(new FileTransferFailure(
                    new EventMetadata { JobName = jobName },
                    fileInfo,
                    e.Message,
                    beforeTransfer
                ));
                continue;
            }
            finally
            {
                gate?.Release(extension, fileSize);
            }

            remainingCount--; remainingSize -= fileSize;

            TimeSpan encryptTime = TimeSpan.Zero;
            if (typesToEncrypt.Contains(Path.GetExtension(targetFile)))
            {
                try
                {
                    encryptTime = _encryptionService.Encrypt(targetFile, encryptKey);
                }
                catch (FileNotFoundException)
                {
                    throw; // CryptoSoft not installed — abort the backup
                }
                catch (Exception e)
                {
                    failedCount++;
                    Notify(new FileTransferFailure(
                        new EventMetadata { JobName = jobName },
                        fileInfo,
                        e.Message,
                        beforeTransfer
                    ));
                    continue;
                }
            }

            Notify(new FileTransferSuccess(
                    new EventMetadata { JobName = jobName },
                    fileInfo, transferTime, encryptTime,
                    new BackupProgress(count, remainingCount, size, remainingSize)
                ));

            if (IsSoftwareRunning(jobName)) { interrupted = true; break; }
        }

        if (!interrupted)
        {
            Notify(new BackupCompleted(
                new EventMetadata { JobName = jobName },
                count, size, failedCount
            ));
        }
    }

}
