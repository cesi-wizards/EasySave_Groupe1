namespace EasySave.Domain.Entities;

public class Context
{
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


}
