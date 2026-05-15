using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Entities;

public class BackupJob(string name, string source, string target, IBackupStrategy strategy, List<string> TypesToEncrypt, string encryptKey)
{
    public string Name { get; init; } = name;
    public string Source { get; init; } = source;
    public string Target { get; init; } = target;
    public IBackupStrategy Strategy { get; init; } = strategy;
    public List<string> TypesToEncrypt { get; init; } = TypesToEncrypt;
    public string EncryptKey { get; init; } = encryptKey;

    public void Execute(ManualResetEvent pauseEvent, ITransferGate gate)
    {
        Strategy.Execute(Name, Source, Target, TypesToEncrypt, EncryptKey, pauseEvent, gate);
    }
}
