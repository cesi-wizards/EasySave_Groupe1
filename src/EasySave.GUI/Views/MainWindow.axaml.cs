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

    private void OnSettingsNavClick(object? sender, RoutedEventArgs _)
    {
        ((MainWindowViewModel)DataContext!).CurrentPage = MainWindowViewModel.AppPage.Settings;
    }

    private void OnJobsNavClick(object? sender, RoutedEventArgs _)
    {
        ((MainWindowViewModel)DataContext!).CurrentPage = MainWindowViewModel.AppPage.Jobs;
    }

    private void OnLogsNavClick(object? sender, RoutedEventArgs _)
    {
        ((MainWindowViewModel)DataContext!).CurrentPage = MainWindowViewModel.AppPage.Logs;
    }

}
