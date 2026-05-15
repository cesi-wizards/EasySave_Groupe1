using EasySave.Domain.Interfaces;

namespace EasySave.Application;

public class TransferGate : ITransferGate
{
    private readonly ManualResetEventSlim _priorityGate = new(true);
    private readonly SemaphoreSlim _largeFileLock = new(1, 1);
    private int _priorityPendingCount;

    private HashSet<string> _priorityExtensions = [];
    private long _largeFileSizeThresholdBytes;                        // 0 = disable

    public void SetPriorityExtensions(IEnumerable<string> extensions)
    {
        _priorityExtensions = extensions
            .Select(e => e.StartsWith('.') ? e : $".{e}")
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public void SetLargeFileSizeThreshold(long thresholdKb)
    {
        _largeFileSizeThresholdBytes = thresholdKb * 1024;
    }

    public void RegisterPriorityFiles(int count)
    {
        if (count <= 0) return;
        Interlocked.Add(ref _priorityPendingCount, count);
        _priorityGate.Reset();
    }

    public void Acquire(string extension, long fileSizeBytes)
    {
        if (!IsPriority(extension))
            _priorityGate.Wait();

        if (IsLarge(fileSizeBytes))
            _largeFileLock.Wait();
    }

    public void Release(string extension, long fileSizeBytes)
    {
        if (IsLarge(fileSizeBytes))
            _largeFileLock.Release();

        if (IsPriority(extension))
        {
            int remaining = Interlocked.Decrement(ref _priorityPendingCount);
            if (remaining <= 0)
                _priorityGate.Set();
        }
    }

    public bool IsPriority(string extension) =>
        _priorityExtensions.Count > 0 && _priorityExtensions.Contains(extension);

    private bool IsLarge(long fileSizeBytes) =>
        _largeFileSizeThresholdBytes > 0 && fileSizeBytes > _largeFileSizeThresholdBytes;
}
