using Avalonia.Controls;
using Avalonia.Interactivity;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OnNewBackupClick(object? sender, RoutedEventArgs _)
    {
        var dialog = new BackupConfigDialog();
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed && dialog.DataContext is BackupConfigDialogViewModel dialogVm && dialogVm.Result is not null)
            ((MainWindowViewModel)DataContext!).AddBackupConfig(dialogVm.Result);
    }

    private async void OnSettingsClick(object? sender, RoutedEventArgs _)
    {
        var vm = (MainWindowViewModel)DataContext!;
        var dialog = new SettingsDialog(vm.BlockingApp, vm.LogFileType);
        var confirmed = await dialog.ShowDialog<bool>(this);

        if (confirmed && dialog.DataContext is SettingsDialogViewModel settingsVm)
        {
            vm.BlockingApp = settingsVm.BlockingApp;
            vm.LogFileType = settingsVm.SelectedLogFileType;
        }
    }
}
