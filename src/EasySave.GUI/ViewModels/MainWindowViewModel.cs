using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.Domain.Entities;
using JobManager = EasySave.Application.JobManager;

namespace EasySave.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly JobManager _jobManager = new();

    public AppSettings Settings { get; }
    public JobsPageViewModel JobsPageVm { get; }
    public SettingsPageViewModel SettingsPageVm { get; }
    public LogsPageViewModel LogsPageVm { get; }

    public enum AppPage { Jobs, Logs, Settings }

    [ObservableProperty] private AppPage _currentPage = AppPage.Jobs;

    public bool ShowJobsPage     => CurrentPage == AppPage.Jobs;
    public bool ShowLogsPage     => CurrentPage == AppPage.Logs;
    public bool ShowSettingsPage => CurrentPage == AppPage.Settings;

    public string CurrentPageTitle => CurrentPage switch
    {
        AppPage.Logs     => Localization["LogsNav"],
        AppPage.Settings => Localization["SettingsTitle"],
        _                => Localization["BackupJobsNav"],
    };

    public int TotalJobs   => JobsPageVm.TotalJobs;
    public int DoneJobs    => JobsPageVm.DoneJobs;
    public int RunningJobs => JobsPageVm.RunningJobs;

    partial void OnCurrentPageChanged(AppPage value)
    {
        OnPropertyChanged(nameof(ShowJobsPage));
        OnPropertyChanged(nameof(ShowLogsPage));
        OnPropertyChanged(nameof(ShowSettingsPage));
        OnPropertyChanged(nameof(CurrentPageTitle));
        if (value == AppPage.Logs) LogsPageVm.Refresh();
    }

    public MainWindowViewModel()
    {
        Settings = new AppSettings();
        JobsPageVm = new JobsPageViewModel(_jobManager, Settings);
        SettingsPageVm = new SettingsPageViewModel(_jobManager, Settings);
        LogsPageVm = new LogsPageViewModel();

        JobsPageVm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(JobsPageViewModel.TotalJobs)
                               or nameof(JobsPageViewModel.DoneJobs)
                               or nameof(JobsPageViewModel.RunningJobs))
                OnPropertyChanged(e.PropertyName);
        };

        Localization.PropertyChanged += (_, _) => OnPropertyChanged(nameof(CurrentPageTitle));
    }

    public void AddBackupConfig(BackupConfig config) => JobsPageVm.AddBackupConfig(config);
}
