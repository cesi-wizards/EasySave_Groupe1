using System.Threading.Tasks;
using System.Collections.Concurrent;

using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Infrastructure.Factories;
using EasySave.Infrastructure.Factories.Interfaces;
using EasySave.Infrastructure.Services;
using EasySave.Infrastructure.Subscribers;

namespace EasySave.Application;

public class JobManager
{
    public List<BackupJob> Jobs { get; private set; } = [];

    private readonly List<string> _businessSoftwares;
    private readonly ISoftwareDetector _softwareDetector;

    private readonly ConcurrentDictionary<string, Task> _runningJobs = new();
    private readonly ConcurrentDictionary<string, ManualResetEvent> _pauseEvents = new();

    public JobManager(List<string>? businessSoftwares = null)
    {
        _businessSoftwares = businessSoftwares ?? [];
        _softwareDetector = new SoftwareDetector(_businessSoftwares);
    }

    public void AddJob(BackupConfig config, ISubscriber? extraSubscriber = null)
    {
        ISubscriber stateTracker = new StateTracker();
        ISubscriber dailyLogger = new DailyLogger(config.LogFileType);

        List<ISubscriber> subscribers = [stateTracker, dailyLogger];
        if (extraSubscriber is not null)
            subscribers.Add(extraSubscriber);

        IBackupFactory backupFactory;

        if (config.Type == BackupType.Full)
        {
            backupFactory = new FullBackupFactory(subscribers, _softwareDetector);
        }
        else
        {
            backupFactory = new DifferentialBackupFactory(subscribers, _softwareDetector);
        }
        BackupJob jobToAdd = backupFactory.CreateJob(config.Name, config.SourcePath, config.TargetPath, config.TypesToEncrypt, config.EncryptKey);
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

    public void SetBusinessSoftwares(IEnumerable<string> businessSoftwares)
    {
        _businessSoftwares.Clear();
        _businessSoftwares.AddRange(businessSoftwares);
        _softwareDetector.UpdateProcessNames(_businessSoftwares);
    }

    public Task ExecuteJob(string name)
    {
        if (_runningJobs.ContainsKey(name))
        {
            return _runningJobs[name];
        }

        BackupJob? job = Jobs.Find(j => j.Name == name);
        if (job == null)
        {
            return Task.CompletedTask;
        }

        var pauseEvent = _pauseEvents.GetOrAdd(name, _ => new ManualResetEvent(true));

        var task = Task.Run(() =>
        {
            try
            {
                job.Execute(pauseEvent);
            }
            finally
            {
                _runningJobs.TryRemove(name, out _);
                _pauseEvents.TryRemove(name, out _);
            }
        });

        _runningJobs[name] = task;
        return task;
    }

    public void PauseJob(string name)
    {
        if (_pauseEvents.TryGetValue(name, out var pauseEvent))
        {
            pauseEvent.Reset();
        }
    }

    public void ResumeJob(string name)
    {
        if (_pauseEvents.TryGetValue(name, out var pauseEvent))
        {
            pauseEvent.Set();
        }
    }

    public void ExecuteJobs()
    {
        foreach (BackupJob job in Jobs)
        {
            var pauseEvent = _pauseEvents.GetOrAdd
            (
                job.Name, _ => new ManualResetEvent(true)
            );
            job.Execute(pauseEvent);
        }
    }
}
