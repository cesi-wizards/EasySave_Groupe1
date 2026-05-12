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

    private readonly ConcurrentDictionary<string, Lazy<Task>> _runningJobs = new();
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
            _runningJobs.TryRemove(backupName, out _);
            // _pauseEvents intentionally not touched : la task en cours possède son MRE et le dispose dans son finally
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
        BackupJob? job = Jobs.Find(j => j.Name == name);
        if (job == null) return Task.CompletedTask;

        var lazy = _runningJobs.GetOrAdd(name, _ => new Lazy<Task>(() =>
        {
            var pauseEvent = new ManualResetEvent(true);
            _pauseEvents[name] = pauseEvent;

            return Task.Run(() =>
            {
                try
                {
                    job.Execute(pauseEvent);
                }
                finally
                {
                    _runningJobs.TryRemove(name, out Lazy<Task> _);
                    // Ne retire l'entrée que si c'est bien notre MRE (pas celui d'une nouvelle exécution)
                    ((ICollection<KeyValuePair<string, ManualResetEvent>>)_pauseEvents)
                        .Remove(new KeyValuePair<string, ManualResetEvent>(name, pauseEvent));
                    pauseEvent.Dispose();
                }
            });
        }));

        return lazy.Value;
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

    public Task ExecuteAllJobs()
    {
        var tasks = Jobs.Select(job => ExecuteJob(job.Name));
        return Task.WhenAll(tasks);

    }
}
