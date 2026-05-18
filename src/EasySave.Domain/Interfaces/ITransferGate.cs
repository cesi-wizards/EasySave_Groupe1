namespace EasySave.Domain.Interfaces;

public interface ITransferGate
{
    bool IsPriority(string extension);
    void RegisterPriorityFiles(int count);
    void Acquire(string extension, long fileSizeBytes);
    void Release(string extension, long fileSizeBytes);
}
