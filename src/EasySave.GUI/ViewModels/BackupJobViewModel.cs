using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.Domain.Entities;
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

    public void Update(Context ctx)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (ctx.TotalCount > 0)
                Progress = (int)((ctx.TotalCount - ctx.RemainingCount) * 100.0 / ctx.TotalCount);
            CurrentFile = System.IO.Path.GetFileName(ctx.SourcePath);
        });
    }
}
