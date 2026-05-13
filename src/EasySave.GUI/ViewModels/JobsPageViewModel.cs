using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.Input;
using EasySave.Application;
using EasySave.Domain.Entities;

namespace EasySave.GUI.ViewModels;

public partial class JobsPageViewModel : ViewModelBase
{
    private readonly JobManager _jobManager;
    private readonly AppSettings _settings;

    public JobsPageViewModel(JobManager jobManager, AppSettings settings)
    {
        _jobManager = jobManager;
        _settings = settings;
        BackupJobs.CollectionChanged += OnJobsChanged;
    }

    public ObservableCollection<BackupJobViewModel> BackupJobs { get; } = [];

    public int TotalJobs   => BackupJobs.Count;
    public int DoneJobs    => BackupJobs.Count(j => j.Status == JobStatus.Done);
    public int RunningJobs => BackupJobs.Count(j => j.Status == JobStatus.Running);

    public bool HasJobs    => BackupJobs.Count > 0;
    public bool HasNoJobs  => BackupJobs.Count == 0;
    public bool HasRunning => RunningJobs > 0;

    private void OnJobsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
            foreach (BackupJobViewModel job in e.NewItems)
                job.PropertyChanged += (_, ev) =>
                {
                    if (ev.PropertyName == nameof(BackupJobViewModel.Status))
                        RefreshStats();
                };
        RefreshStats();
    }

    private void RefreshStats()
    {
        OnPropertyChanged(nameof(TotalJobs));
        OnPropertyChanged(nameof(DoneJobs));
        OnPropertyChanged(nameof(RunningJobs));
        OnPropertyChanged(nameof(HasJobs));
        OnPropertyChanged(nameof(HasNoJobs));
        OnPropertyChanged(nameof(HasRunning));
        ExecuteAllJobsCommand.NotifyCanExecuteChanged();
    }

    public void AddBackupConfig(BackupConfig config)
    {
        config.LogFileType = _settings.LogFileType;
        var jobVm = new BackupJobViewModel(config);
        BackupJobs.Add(jobVm);
        _jobManager.AddJob(config, jobVm);
    }

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task RunOrPauseJob(BackupJobViewModel jobVm)
    {
        if (jobVm.IsRunning)
        {
            if (jobVm.IsPaused)
            {
                _jobManager.ResumeJob(jobVm.Config.Name);
                jobVm.IsPaused = false;
            }
            else
            {
                _jobManager.PauseJob(jobVm.Config.Name);
                jobVm.IsPaused = true;
            }
            return;
        }

        jobVm.Progress = 0;
        jobVm.CurrentFile = string.Empty;
        jobVm.IsPaused = false;
        jobVm.Status = JobStatus.Running;

        _jobManager.ResumeJob(jobVm.Config.Name);
        await _jobManager.ExecuteJob(jobVm.Config.Name);

        jobVm.Status = JobStatus.Idle;
        jobVm.IsPaused = false;
    }

    [RelayCommand]
    private void RemoveJob(BackupJobViewModel jobVm)
    {
        _jobManager.RemoveJob(jobVm.Config.Name);
        BackupJobs.Remove(jobVm);
    }

    private bool CanExecuteAllJobs() => BackupJobs.Any(j => j.Status != JobStatus.Running);

    [RelayCommand(AllowConcurrentExecutions = true, CanExecute = nameof(CanExecuteAllJobs))]
    private Task ExecuteAllJobs()
    {
        var tasks = BackupJobs
            .Where(j => j.Status != JobStatus.Running)
            .Select(j => RunOrPauseJob(j));
        return Task.WhenAll(tasks);
    }
}
