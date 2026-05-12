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

    public bool IsJsonLog
    {
        get => SelectedLogFileType == "JSON";
        set { if (value) SelectedLogFileType = "JSON"; }
    }

    public bool IsXmlLog
    {
        get => SelectedLogFileType == "XML";
        set { if (value) SelectedLogFileType = "XML"; }
    }

    partial void OnSelectedLogFileTypeChanged(string value)
    {
        OnPropertyChanged(nameof(IsJsonLog));
        OnPropertyChanged(nameof(IsXmlLog));
    }

    public event Action<bool>? CloseRequested;

    public SettingsDialogViewModel() : this(string.Empty, "JSON") { }

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
