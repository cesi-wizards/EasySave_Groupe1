using Avalonia.Controls;
using Avalonia.Dialogs;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views;

public partial class BackupConfigDialog : Window
{
    public BackupConfigDialog()
    {
        InitializeComponent();

        var vm = new BackupConfigDialogViewModel();
        vm.CloseRequested += confirmed => Close(confirmed);
        DataContext = vm;
    }

    private async void SelectFolderButton_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button button) return;
        if (DataContext is not BackupConfigDialogViewModel vm) return;

        var topLevel = this;

        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions
        {
            Title = "Select a Folder",
            AllowMultiple = false
        });

        if (folder.Count == 0) return;

        switch (button?.Tag?.ToString())
        {
            case "SourcePath":
                vm.SourcePath = folder[0].Path.LocalPath;
                break;
            case "TargetPath":
                vm.TargetPath = folder[0].Path.LocalPath;
                break;
        }
    }
}
