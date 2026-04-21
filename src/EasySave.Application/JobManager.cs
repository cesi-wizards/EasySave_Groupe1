using EasySave.Domain.Entities;
using EasySave.Infrastructure.Factories;
using EasySave.Infrastructure.Factories.Interfaces;

namespace EasySave.Application;

public class JobManager
{
    public List<BackupJob> Jobs { get; set; } = [];

    public void AddJob(BackupConfig config)
    {
        IBackupFactory backupFactory;

        if (config.Type == BackupType.Full)
        {
            backupFactory = new FullBackupFactory();
        }
        else
        {
            backupFactory = new DifferentialBackupFactory();
        }

        BackupJob jobToAdd = backupFactory.CreateJob(config.Name, config.SourcePath, config.TargetPath);
        Jobs.Add(jobToAdd);
    }

    public void RemoveJob(string backupName)
    {
        var jobToRemove = Jobs.Find(job => job.Name == backupName);
        if (jobToRemove != null)
        {
            Jobs.Remove(jobToRemove);
        }
    }

    public void ExecuteJobs()
    {
        foreach (BackupJob job in Jobs)
        {
            job.Execute();
        }

    }
}
