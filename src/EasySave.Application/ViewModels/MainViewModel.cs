using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Domain.Entities;
using EasySave.Infrastructure.Parsers;

namespace EasySave.Application.ViewModels;

public class MainViewModel(string configFilePath, int[] jobsToExecute)
{
    private List<BackupConfig> _configs { get; set; } = [];
    private string ConfigFilePath { get; } = Path.Combine(Directory.GetCurrentDirectory(), configFilePath);
    private int[] JobsToExecute { get; init; } = jobsToExecute;

    public void CreateBackupConfig()
    {
        string jsonContent = File.ReadAllText(ConfigFilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() }
        };

        FileConfig totalConfig = JsonSerializer.Deserialize<FileConfig>(jsonContent, options);

        if (totalConfig == null) throw new Exception("No config file found");
        foreach (int job in JobsToExecute)
        {
            _configs.Add(totalConfig.Jobs[job - 1]);
        }
    }

    public void Execute()
    {
        var jobManager = new JobManager();
        foreach (BackupConfig config in _configs)
        {
            jobManager.AddJob(config);
        }
        jobManager.ExecuteJobs();
    }
}
