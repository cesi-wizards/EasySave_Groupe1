using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.Domain.Entities;
using EasySave.Domain.Events;
using EasySave.Domain.Interfaces;

namespace EasySave.GUI.ViewModels;

public enum JobStatus { Idle, Running, Done, Error }

public partial class BackupJobViewModel : ViewModelBase, ISubscriber
{
    public BackupConfig Config { get; }

    [ObservableProperty] private int _progress;
    [ObservableProperty] private string _currentFile = string.Empty;
    [ObservableProperty] private JobStatus _status = JobStatus.Idle;

    [ObservableProperty] private bool _isPaused;
    [ObservableProperty] private bool _isRunning;
    [ObservableProperty] private string _pauseButtonText = "▶";

    public BackupJobViewModel(BackupConfig config)
    {
        Config = config;
        UpdatePauseButtonText();
    }

    partial void OnIsPausedChanged(bool value) => UpdatePauseButtonText();
    partial void OnIsRunningChanged(bool value) => UpdatePauseButtonText();

    private void UpdatePauseButtonText()
    {
        PauseButtonText = (!IsRunning || IsPaused) ? "▶" : "||";
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
                IsRunning = false;
                IsPaused = false;
            });
        }
        else if (backupEvent is BackupCompleted)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                Progress = 100;
                CurrentFile = string.Empty;
                IsRunning = false;
                IsPaused = false;
            });
        }
    }

    // ── Status badge ──────────────────────────────────────────────────────────

    public bool IsRunning => Status == JobStatus.Running;

    public string StatusLabel => Status switch
    {
        JobStatus.Running => "Running",
        JobStatus.Done    => "Done",
        JobStatus.Error   => "Error",
        _                 => "Idle",
    };

    public IBrush StatusBackground => Status switch
    {
        JobStatus.Running => new SolidColorBrush(Color.FromArgb(0x1E, 0x3B, 0x82, 0xF6)),
        JobStatus.Done    => new SolidColorBrush(Color.FromArgb(0x1E, 0x22, 0xC5, 0x5E)),
        JobStatus.Error   => new SolidColorBrush(Color.FromArgb(0x1E, 0xEF, 0x44, 0x44)),
        _                 => new SolidColorBrush(Color.FromArgb(0x0F, 0x00, 0x00, 0x00)),
    };

    public IBrush StatusForeground => Status switch
    {
        JobStatus.Running => new SolidColorBrush(Color.Parse("#3B82F6")),
        JobStatus.Done    => new SolidColorBrush(Color.Parse("#16A34A")),
        JobStatus.Error   => new SolidColorBrush(Color.Parse("#DC2626")),
        _                 => new SolidColorBrush(Color.Parse("#6B7280")),
    };

    public IBrush ProgressColor => Status switch
    {
        JobStatus.Done  => new SolidColorBrush(Color.Parse("#22C55E")),
        JobStatus.Error => new SolidColorBrush(Color.Parse("#EF4444")),
        _               => new SolidColorBrush(Color.Parse("#3B6AF7")),
    };

    // ── Type chip ─────────────────────────────────────────────────────────────

    public string TypeLabel => Config.Type.ToString();

    public IBrush TypeBackground => Config.Type == BackupType.Full
        ? new SolidColorBrush(Color.FromArgb(0x1A, 0x8B, 0x5C, 0xF6))
        : new SolidColorBrush(Color.FromArgb(0x1A, 0xF5, 0x9E, 0x0B));

    public IBrush TypeForeground => Config.Type == BackupType.Full
        ? new SolidColorBrush(Color.Parse("#7C3AED"))
        : new SolidColorBrush(Color.Parse("#B45309"));

    // ── Extras ────────────────────────────────────────────────────────────────

    public bool HasEncryptKey => !string.IsNullOrEmpty(Config.EncryptKey);

    public string ExtensionsText => Config.TypesToEncrypt.Count > 0
        ? string.Join(" ", Config.TypesToEncrypt.Take(2))
          + (Config.TypesToEncrypt.Count > 2 ? $" +{Config.TypesToEncrypt.Count - 2}" : "")
        : string.Empty;

    public bool HasExtensions => Config.TypesToEncrypt.Count > 0;

    partial void OnStatusChanged(JobStatus value)
    {
        OnPropertyChanged(nameof(IsRunning));
        OnPropertyChanged(nameof(StatusLabel));
        OnPropertyChanged(nameof(StatusBackground));
        OnPropertyChanged(nameof(StatusForeground));
        OnPropertyChanged(nameof(ProgressColor));
    }
}
