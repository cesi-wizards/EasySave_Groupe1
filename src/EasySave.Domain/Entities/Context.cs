namespace EasySave.Domain.Entities;

public class Context(string jobName, DateTime timestamp, string sourcePath, string targetPath,
    long fileSize, TimeSpan transferTime, int totalCount, int remainingCount, long totalSize, long remainingSize)
{
    public string JobName { get; init; } = jobName;
    public DateTime Timestamp { get; init; } = timestamp;
    public string SourcePath { get; init;  } = sourcePath;
    public string TargetPath { get; init; } = targetPath;
    public long FileSize { get; init; } = fileSize;
    public TimeSpan TransferTime { get; init; } = transferTime;
    public int TotalCount { get; init; } = totalCount;
    public int RemainingCount { get; init; } = remainingCount;
    public long TotalSize { get; init; } = totalSize;
    public long RemainingSize { get; init; } = remainingSize;
}
