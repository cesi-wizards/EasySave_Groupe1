using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views;

public partial class JobsPage : UserControl
{
    public JobsPage()
    {
        InitializeComponent();
    }

    private async void OnNewBackupClick(object? sender, RoutedEventArgs _)
    {
        var window = TopLevel.GetTopLevel(this) as Window;
        if (window is null) return;

        var dialog = new BackupConfigDialog();
        var confirmed = await dialog.ShowDialog<bool>(window);

        if (confirmed && dialog.DataContext is BackupConfigDialogViewModel dialogVm && dialogVm.Result is not null)
            ((JobsPageViewModel)DataContext!).AddBackupConfig(dialogVm.Result);
    }
}

