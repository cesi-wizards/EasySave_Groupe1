using EasySave.Domain.Entities;

namespace EasySave.Domain.Strategies;

public class FullBackupStrategy : AbstractBackupStrategy
{
    private (List<string>, int, long) GetFilesToBackup(string sourcePath)
    {
        var toBackup = new List<string>();
        int count = 0; long size = 0;

        foreach (var sourceFile in GetFiles(sourcePath))
        {
            toBackup.Add(sourceFile);
            count++;
            size += new FileInfo(sourceFile).Length;
        }
        return (toBackup, count, size);
    }
    public override void Execute(string JobName, string sourcePath, string targetPath)
    {
        var (toBackup, count, size) = GetFilesToBackup(sourcePath);
        int remainingCount = count; long remainingSize = size;

        foreach (var sourceFile in toBackup)
        {
            long fileSize = new FileInfo(sourceFile).Length;

            var relativePath = Path.GetRelativePath(sourcePath, sourceFile);
            var targetFile = Path.Combine(targetPath, relativePath);

            Context contextPreBackup = new Context(jobName: JobName, timestamp: DateTime.Now,
                sourcePath: sourcePath, targetPath: targetPath, fileSize: fileSize, transferTime: TimeSpan.Zero,
                totalCount: count, totalSize: size, remainingCount: remainingCount, remainingSize: remainingSize);
            Notify(contextPreBackup);

            var transferTime = CopyFile(sourceFile, targetFile);
            remainingCount--; remainingSize -= fileSize;

            Context contextPostBackup = new Context(jobName: JobName, timestamp: DateTime.Now,
                sourcePath: sourcePath, targetPath: targetPath, fileSize: fileSize, transferTime: transferTime,
                totalCount: count, totalSize: size, remainingCount: remainingCount, remainingSize: remainingSize);
            Notify(contextPostBackup);
        }
    }
}
