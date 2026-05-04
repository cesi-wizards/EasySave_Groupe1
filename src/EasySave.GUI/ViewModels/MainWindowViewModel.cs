using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Domain.Entities;
using JobManager = EasySave.Application.JobManager;

namespace EasySave.GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly JobManager _jobManager = new();

    [ObservableProperty] private string _blockingApp = string.Empty;
    [ObservableProperty] private string _logFileType = "JSON";

    public ObservableCollection<BackupJobViewModel> BackupJobs { get; } = [];

    public void AddBackupConfig(BackupConfig config)
    {
        config.LogFileType = LogFileType;
        var jobVm = new BackupJobViewModel(config);
        BackupJobs.Add(jobVm);
        _jobManager.AddJob(config, jobVm);
    }

    partial void OnBlockingAppChanged(string value)
    {
        var softwares = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        _jobManager.SetBusinessSoftwares(softwares);
    }

    [RelayCommand]
    private void RemoveJob(BackupJobViewModel jobVm)
    {
        _jobManager.RemoveJob(jobVm.Config.Name);
        BackupJobs.Remove(jobVm);
    }

    [RelayCommand]
    private async Task ExecuteJob(BackupJobViewModel jobVm)
    {
        jobVm.Progress = 0;
        jobVm.CurrentFile = string.Empty;
        await Task.Run(() => _jobManager.ExecuteJob(jobVm.Config.Name));
    }
}
