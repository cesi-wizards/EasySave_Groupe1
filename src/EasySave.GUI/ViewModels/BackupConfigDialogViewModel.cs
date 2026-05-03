using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Domain.Entities;

namespace EasySave.GUI.ViewModels;

public partial class BackupConfigDialogViewModel : ViewModelBase
{
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private string _sourcePath = string.Empty;
    [ObservableProperty] private string _targetPath = string.Empty;
    [ObservableProperty] private BackupType _selectedType = BackupType.Full;
    [ObservableProperty] private string _selectedLogFileType = "JSON";
    [ObservableProperty] private string _extensionInput = string.Empty;
    [ObservableProperty] private string _encryptKey = string.Empty;

    public IEnumerable<BackupType> BackupTypes { get; } = [BackupType.Full, BackupType.Differential];
    public IEnumerable<string> LogFileTypes { get; } = ["JSON", "XML"];
    public ObservableCollection<string> EncryptedExtensions { get; } = [];

    public BackupConfig? Result { get; private set; }

    public event Action<bool>? CloseRequested;

    [RelayCommand]
    private void AddExtension()
    {
        var ext = ExtensionInput.Trim();
        if (string.IsNullOrEmpty(ext)) return;
        if (!ext.StartsWith('.')) ext = "." + ext;
        if (!EncryptedExtensions.Contains(ext))
            EncryptedExtensions.Add(ext);
        ExtensionInput = string.Empty;
    }

    [RelayCommand]
    private void RemoveExtension(string ext) => EncryptedExtensions.Remove(ext);

    [RelayCommand]
    private void Confirm()
    {
        Result = new BackupConfig(Name, SourcePath, TargetPath, SelectedType, SelectedLogFileType, [.. EncryptedExtensions], EncryptKey);
        CloseRequested?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}
