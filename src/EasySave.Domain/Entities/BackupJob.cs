using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Entities;

public class BackupJob(string name, string source, string target, IBackupStrategy strategy)
{
    public string Name { get; init; } = name;
    public string Source { get; init; } = source;
    public string Target { get; init; } = target;
    public IBackupStrategy Strategy { get; init; } = strategy;
    public void Execute()
    {
        Strategy.Execute(Name, Source, Target);
    }
}
