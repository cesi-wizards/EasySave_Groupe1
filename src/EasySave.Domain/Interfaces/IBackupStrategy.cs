namespace EasySave.Domain.Interfaces;

public interface IBackupStrategy
{
    void Execute(string jobName, string sourcePath, string targetPath, List<string> TypesToEncrypt, string encryptKey, ManualResetEvent pause, ITransferGate gate);
}
