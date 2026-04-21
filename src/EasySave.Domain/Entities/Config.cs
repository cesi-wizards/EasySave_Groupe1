namespace EasySave.Domain.Entities;

public class Context
{
    // Attributs of the job's configs
    public string SaveName { get; } = string.Empty;
    public string SourceRepository { get; } = string.Empty;
    public string TargetRepository { get; } = string.Empty;

    // Constructors
    public Config() { }
    public Config(string saveName, string sourceRepository, string targetRepository)
    {
        SaveName = saveName;
        SourceRepository = sourceRepository;
        TargetRepository = targetRepository;
    }
}
