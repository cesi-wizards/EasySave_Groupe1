using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Infrastructure.Factories;
using EasySave.Infrastructure.Factories.Interfaces;
using EasySave.Infrastructure.Subscribers;

namespace EasySave.Application;

public class JobManager
{
    public List<BackupJob> Jobs { get; private set; } = [];

    public void AddJob(BackupConfig config)
    {
        ISubscriber stateTracker = new StateTracker();
        ISubscriber dailyLogger = new DailyLogger(config.LogFileType);

        IBackupFactory backupFactory;

        if (config.Type == BackupType.Full)
        {
            backupFactory = new FullBackupFactory([stateTracker, dailyLogger]);
        }
        else
        {
            backupFactory = new DifferentialBackupFactory([stateTracker, dailyLogger]);
        }
        BackupJob jobToAdd = backupFactory.CreateJob(config.Name, config.SourcePath, config.TargetPath, config.EncryptTypes, config.EncryptKey);
        Jobs.Add(jobToAdd);
    }

    public void RemoveJob(string backupName)
    {
        BackupJob? jobToRemove = Jobs.Find(job => job.Name == backupName);
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
