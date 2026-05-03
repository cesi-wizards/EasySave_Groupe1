using System.Collections.ObjectModel;
using System.Diagnostics;
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

    [RelayCommand]
    private void RemoveJob(BackupJobViewModel jobVm)
    {
        _jobManager.RemoveJob(jobVm.Config.Name);
        BackupJobs.Remove(jobVm);
    }

    [RelayCommand]
    private void ExecuteJob(BackupJobViewModel jobVm)
    {
        if (!string.IsNullOrWhiteSpace(BlockingApp))
        {
            var processes = Process.GetProcessesByName(BlockingApp.Trim());
            if (processes.Length > 0)
                return;
        }

        jobVm.Progress = 0;
        jobVm.CurrentFile = string.Empty;
        _jobManager.ExecuteJob(jobVm.Config.Name);
    }
}
