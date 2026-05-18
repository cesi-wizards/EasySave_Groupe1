namespace EasySave.Domain.Events;

public record EventMetadata
{
    public required string JobName { get; init; }
    public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}

public record BackupFileInfo
(
    string SourcePath,
    long FileSize,
    string TargetPath
);

public record BackupProgress
(
    int TotalCount,
    int RemainingCount,
    long TotalSize,
    long RemainingSize
);
