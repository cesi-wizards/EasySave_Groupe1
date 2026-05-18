using System.Collections.Concurrent;

using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;

namespace EasySave.Application;

/// <summary>
/// Delegate that the Composition Root (CLI/GUI) provides to create the correct
/// IBackupFactory for a given backup type, without coupling Application to Infrastructure.
/// </summary>
public delegate IBackupFactory BackupFactoryBuilder(
    BackupType type,
    List<ISubscriber> subscribers,
    ISoftwareDetector softwareDetector);

public class JobManager
{
    public List<BackupJob> Jobs { get; private set; } = [];

    private readonly ISoftwareDetector _softwareDetector;
    private readonly BackupFactoryBuilder _factoryBuilder;

    private readonly ConcurrentDictionary<string, Lazy<Task>> _runningJobs = new();
    private readonly ConcurrentDictionary<string, ManualResetEvent> _pauseEvents = new();
    private readonly TransferGate _transferGate = new();

    public JobManager(ISoftwareDetector softwareDetector, BackupFactoryBuilder factoryBuilder)
    {
        _softwareDetector = softwareDetector;
        _factoryBuilder = factoryBuilder;
    }

    public void AddJob(BackupConfig config, IEnumerable<ISubscriber> subscribers)
    {
        var subscriberList = subscribers.ToList();
        IBackupFactory factory = _factoryBuilder(config.Type, subscriberList, _softwareDetector);
        BackupJob job = factory.CreateJob(
            config.Name, config.SourcePath, config.TargetPath,
            config.TypesToEncrypt, config.EncryptKey);
        Jobs.Add(job);
    }

    public void RemoveJob(string backupName)
    {
        BackupJob? jobToRemove = Jobs.Find(job => job.Name == backupName);
        if (jobToRemove != null)
        {
            Jobs.Remove(jobToRemove);
            _runningJobs.TryRemove(backupName, out _);
        }
    }

    public void SetBusinessSoftwares(IEnumerable<string> businessSoftwares) =>
        _softwareDetector.UpdateProcessNames(businessSoftwares);

    public void SetPriorityExtensions(IEnumerable<string> extensions) =>
        _transferGate.SetPriorityExtensions(extensions);

    public void SetLargeFileSizeThreshold(long thresholdKb) =>
        _transferGate.SetLargeFileSizeThreshold(thresholdKb);

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
                    job.Execute(pauseEvent, _transferGate);
                }
                finally
                {
                    _runningJobs.TryRemove(name, out Lazy<Task> _);
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
            pauseEvent.Reset();
    }

    public void ResumeJob(string name)
    {
        if (_pauseEvents.TryGetValue(name, out var pauseEvent))
            pauseEvent.Set();
    }

    public Task ExecuteAllJobs()
    {
        var tasks = Jobs.Select(job => ExecuteJob(job.Name));
        return Task.WhenAll(tasks);
    }
}