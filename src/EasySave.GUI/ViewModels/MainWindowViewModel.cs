using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Domain.Entities;
using JobManager = EasySave.Application.JobManager;

namespace EasySave.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly JobManager _jobManager = new();

    public enum AppPage { Jobs, Logs, Settings }

    [ObservableProperty] private AppPage _currentPage = AppPage.Jobs;

    public LogsPageViewModel LogsPageVm { get; } = new();

    public bool ShowJobsPage     => CurrentPage == AppPage.Jobs;
    public bool ShowLogsPage     => CurrentPage == AppPage.Logs;
    public bool ShowSettingsPage => CurrentPage == AppPage.Settings;

    public string CurrentPageTitle => CurrentPage switch
    {
        AppPage.Logs     => Localization["LogsNav"],
        AppPage.Settings => Localization["SettingsTitle"],
        _                => Localization["BackupJobsNav"],
    };

    partial void OnCurrentPageChanged(AppPage value)
    {
        OnPropertyChanged(nameof(ShowJobsPage));
        OnPropertyChanged(nameof(ShowLogsPage));
        OnPropertyChanged(nameof(ShowSettingsPage));
        OnPropertyChanged(nameof(CurrentPageTitle));
        if (value == AppPage.Logs) LogsPageVm.Refresh();
    }

    [ObservableProperty] private string _blockingApp = string.Empty;
    [ObservableProperty] private string _logFileType = "JSON";
    [ObservableProperty] private string _logEmplacement = "local";

    public bool IsJsonLog
    {
        get => LogFileType == "JSON";
        set { if (value) LogFileType = "JSON"; }
    }

    public bool IsXmlLog
    {
        get => LogFileType == "XML";
        set { if (value) LogFileType = "XML"; }
    }

    partial void OnLogFileTypeChanged(string value)
    {
        OnPropertyChanged(nameof(IsJsonLog));
        OnPropertyChanged(nameof(IsXmlLog));
    }

    public bool IsLocalLog
    {
        get => LogEmplacement == "local";
        set { if (value) LogEmplacement = "local"; }
    }

    public bool IsServerLog
    {
        get => LogEmplacement == "server";
        set { if (value) LogEmplacement = "server"; }
    }

    public bool IsServerAndLocalLog
    {
        get => LogEmplacement == "both";
        set { if (value) LogEmplacement = "both"; }
    }

    partial void OnLogEmplacementChanged(string value)
    {
        OnPropertyChanged(nameof(IsServerLog));
        OnPropertyChanged(nameof(IsLocalLog));
        OnPropertyChanged(nameof(IsServerAndLocalLog));
    }

    public ObservableCollection<BackupJobViewModel> BackupJobs { get; } = [];

    public int TotalJobs   => BackupJobs.Count;
    public int DoneJobs    => BackupJobs.Count(j => j.Status == JobStatus.Done);
    public int RunningJobs => BackupJobs.Count(j => j.Status == JobStatus.Running);

    public bool HasJobs   => BackupJobs.Count > 0;
    public bool HasNoJobs => BackupJobs.Count == 0;
    public bool HasRunning => RunningJobs > 0;

    public MainWindowViewModel()
    {
        BackupJobs.CollectionChanged += OnJobsChanged;
        Localization.PropertyChanged += (_, _) => OnPropertyChanged(nameof(CurrentPageTitle));
    }

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
        config.LogFileType = LogFileType;
        var jobVm = new BackupJobViewModel(config);
        BackupJobs.Add(jobVm);
        _jobManager.AddJob(config, jobVm);
    }

    partial void OnBlockingAppChanged(string value)
    {
        var softwares = value.Split(',', System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
        _jobManager.SetBusinessSoftwares(softwares);
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

    [RelayCommand(AllowConcurrentExecutions = true)]
    private async Task ExecuteJob(BackupJobViewModel jobVm)
    {
        if (jobVm.Status == JobStatus.Running) return;
        jobVm.Status = JobStatus.Running;
        jobVm.Progress = 0;
        jobVm.CurrentFile = string.Empty;
        await Task.Run(() => _jobManager.ExecuteJob(jobVm.Config.Name));
        if (jobVm.Status == JobStatus.Running)
            jobVm.Status = JobStatus.Done;
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
