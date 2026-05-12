using System.Text.Json;
using System.Text.Json.Serialization;
using EasySave.Domain.Entities;
using EasySave.Infrastructure.Parsers;

namespace EasySave.Application.ViewModels;

public class MainViewModel(string configFilePath, int[] jobsToExecute)
{
    private List<BackupConfig> _configs { get; set; } = [];
    private List<string> _businessSoftwares { get; set; } = [];
    private string ConfigFilePath { get; } = Path.Combine(Directory.GetCurrentDirectory(), configFilePath);
    private int[] JobsToExecute { get; init; } = jobsToExecute;

    public void CreateBackupConfig()
    {
        string jsonContent = File.ReadAllText(ConfigFilePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true, Converters = { new JsonStringEnumConverter() }
        };

        FileConfig totalConfig = JsonSerializer.Deserialize<FileConfig>(jsonContent, options)
            ?? throw new InvalidOperationException("Failed to deserialize config file");

        if (totalConfig == null) throw new Exception("No config file found");
        foreach (int job in JobsToExecute)
        {
            BackupConfig config = totalConfig.Jobs[job - 1];

            config.LogFileType = totalConfig.LogFileType;

            _configs.Add(config);
        }

        _businessSoftwares = totalConfig.BusinessSoftwares ?? [];
    }

    public void Execute()
    {
        var jobManager = new JobManager(_businessSoftwares);
        foreach (BackupConfig config in _configs)
        {
            jobManager.AddJob(config);
        }
        jobManager.ExecuteAllJobs();
    }
}
