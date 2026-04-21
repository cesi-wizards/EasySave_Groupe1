namespace EasySave.Domain.Entities;

public class Context
{
    // Attributs
    public string JobName { get; } = string.Empty;
    public DateTime Timestamp { get; }
    public string SourcePath { get; } = string.Empty;
    public string TargetPath { get; } = string.Empty;
    public double FileSize { get; }
    public float TransferTime { get; }
    public int TotalCount { get; }
    public int RemainingCount { get; }
    public int TotalSize { get; }
    public int RemainingSize { get; }

    // Constructors
    public Context() { }
    public Context(string jobName, DateTime timestamp, string sourcePath, string targetPath, double fileSize, float transferTime, int totalCount, int remainingCount, int totalSize, int remainingSize)
    {
        JobName = jobName;
        Timestamp = timestamp;
        SourcePath = sourcePath;
        TargetPath = targetPath;
        FileSize = fileSize;
        TransferTime = transferTime;
        TotalCount = totalCount;
        RemainingCount = remainingCount;
        TotalSize = totalSize;
        RemainingSize = remainingSize;
    }
}
