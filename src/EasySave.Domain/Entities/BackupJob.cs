using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Entities;

public class BackupJob(string name, string source, string target, IBackupStrategy strategy)
{
    required public string Name { get; init; } = name;
    required public string Source { get; init; } = source;
    required public string Target { get; init; } = target;
    required public IBackupStrategy Strategy { get; init; } = strategy;
    public void Execute()
    {
        Strategy.Execute(Source, Target);
    }
}
