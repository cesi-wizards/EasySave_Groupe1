namespace EasySave.Domain.Entities;

public class Context(string jobName, DateTime dateJob, string sourcePath, string targetPath,
    double fileSize, float transferTime, int totalCount, int remainingCount, int totalSize, int remainingSize)
{
    // Attributs
    public string JobName { get; init; } = jobName;
    public DateTime DateJob { get; init; } = dateJob;
    public string SourcePath { get; init;  } = sourcePath;
    public string TargetPath { get; init; } = targetPath;
    public double FileSize { get; init; } = fileSize;
    public float TransferTime { get; init; } = transferTime;
    public int TotalCount { get; init; } = totalCount;
    public int RemainingCount { get; init; } = remainingCount;
    public int TotalSize { get; init; } = totalSize;
    public int RemainingSize { get; init; } = remainingSize;
}
