using System.Diagnostics;
using EasySave.Domain.Entities;

namespace EasySave.Domain.Strategies;

public class DifferentialBackupStrategy : AbstractBackupStrategy
{
    private (List<string>, int, long) GetFilesToBackup(string sourcePath, string targetPath)
    {
        var toBackup = new List<string>();
        int count = 0; long size = 0;

        foreach (string sourceFile in GetFiles(sourcePath)) // GetFiles() returns a generator so it won't load unused files into memory
        {
            string targetFile = GetTargetFile(sourcePath, targetPath, sourceFile);

            bool needsBackup = true;
            if (File.Exists(targetFile))
            {
                var sourceLastWrite = File.GetLastWriteTime(sourceFile);
                var targetLastWrite = File.GetLastWriteTime(targetFile);
                if (targetLastWrite >= sourceLastWrite)
                {
                    needsBackup = false;
                }
            }
            if (needsBackup)
            {
                toBackup.Add(sourceFile);
                count++;
                size += new FileInfo(sourceFile).Length;
            }
        }
        return (toBackup, count, size);
    }
    public override void Execute(string JobName, string sourcePath, string targetPath)
    {
        var (toBackup, count, size) = GetFilesToBackup(sourcePath, targetPath);
        int remainingCount = count; long remainingSize = size;

        foreach (string sourceFile in toBackup)
        {
            long fileSize = new FileInfo(sourceFile).Length;

            string targetFile = GetTargetFile(sourcePath, targetPath, sourceFile);

            Context contextPreBackup = new Context( jobName: JobName, timestamp: new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(),
                sourcePath: sourcePath, targetPath: targetPath, fileSize: fileSize, transferTime: TimeSpan.Zero,
                totalCount: count, totalSize: size, remainingCount: remainingCount, remainingSize: remainingSize);
            Notify(contextPreBackup);

            TimeSpan transferTime = CopyFile(sourceFile, targetFile);
            remainingCount--; remainingSize -= fileSize;

            Context contextPostBackup = new Context(jobName: JobName, timestamp: new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds(),
                sourcePath: sourcePath, targetPath: targetPath, fileSize: fileSize, transferTime: transferTime,
                totalCount: count, totalSize: size, remainingCount: remainingCount, remainingSize: remainingSize);
            Notify(contextPostBackup);
        }
    }
}
