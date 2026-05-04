using EasySave.GUI.ViewModels;

namespace EasySave.GUI.Views;

public partial class SettingsDialog : Avalonia.Controls.Window
{
    public SettingsDialog() : this(string.Empty, "JSON") { }

    public SettingsDialog(string currentBlockingApp, string currentLogFileType)
    {
        InitializeComponent();

        var vm = new SettingsDialogViewModel(currentBlockingApp, currentLogFileType);
        vm.CloseRequested += confirmed => Close(confirmed);
        DataContext = vm;
    }
}
