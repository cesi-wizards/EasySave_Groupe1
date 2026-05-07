using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.Domain.Entities;
using EasySave.Domain.Events;
using EasySave.Domain.Interfaces;

namespace EasySave.GUI.ViewModels;

public partial class BackupJobViewModel : ViewModelBase, ISubscriber
{
    public BackupConfig Config { get; }

    [ObservableProperty] private int _progress;
    [ObservableProperty] private string _currentFile = string.Empty;

    public BackupJobViewModel(BackupConfig config)
    {
        Config = config;
    }

    public void Update(IBackupEvent backupEvent)
    {
        void UpdateUi(BackupProgress progress, BackupFileInfo file)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (progress.TotalCount > 0)
                    Progress = (int)((progress.TotalCount - progress.RemainingCount) * 100.0 / progress.TotalCount);

                CurrentFile = System.IO.Path.GetFileName(file.SourcePath);
            });
        }

        if (backupEvent is FileTransferReady ready)
        {
            UpdateUi(ready.Progress, ready.File);
        }
        else if (backupEvent is FileTransferSuccess success)
        {
            UpdateUi(success.Progress, success.File);
        }
        else if (backupEvent is FileTransferFailure failure)
        {
            UpdateUi(failure.Progress, failure.File);
        }
        else if (backupEvent is BackupInterrupted)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                CurrentFile = string.Empty;
            });
        }
        else if (backupEvent is BackupCompleted)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Progress = 100;
                CurrentFile = string.Empty;
            });
        }
    }
}
