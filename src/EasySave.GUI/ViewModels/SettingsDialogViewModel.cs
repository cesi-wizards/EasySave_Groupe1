using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EasySave.GUI.ViewModels;

public partial class SettingsDialogViewModel : ViewModelBase
{
    [ObservableProperty] private string _blockingApp = string.Empty;
    [ObservableProperty] private string _selectedLogFileType = "JSON";

    public IEnumerable<string> LogFileTypes { get; } = ["JSON", "XML"];

    public event Action<bool>? CloseRequested;

    public SettingsDialogViewModel(string currentBlockingApp, string currentLogFileType)
    {
        _blockingApp = currentBlockingApp;
        _selectedLogFileType = currentLogFileType;
    }

    [RelayCommand]
    private void Confirm() => CloseRequested?.Invoke(true);

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}
