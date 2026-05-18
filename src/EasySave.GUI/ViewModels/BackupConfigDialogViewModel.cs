using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EasySave.Domain.Entities;

namespace EasySave.GUI.ViewModels;

public partial class BackupConfigDialogViewModel : ViewModelBase
{
    [ObservableProperty]
    [Required]
    private string _name = string.Empty;

    [ObservableProperty]
    [Required]
    private string _sourcePath = string.Empty;

    [ObservableProperty]
    [Required]
    private string _targetPath = string.Empty;

    [ObservableProperty] private BackupType _selectedType = BackupType.Full;
    [ObservableProperty] private string _selectedLogFileType = "JSON";
    [ObservableProperty] private string _extensionInput = string.Empty;
    [ObservableProperty] private string _encryptKey = string.Empty;

    public IEnumerable<BackupType> BackupTypes { get; } = [BackupType.Full, BackupType.Differential];
    public IEnumerable<string> LogFileTypes { get; } = ["JSON", "XML"];
    public ObservableCollection<string> EncryptedExtensions { get; } = [];

    // ── Segmented control helpers ─────────────────────────────────────────────

    public bool IsFullBackup
    {
        get => SelectedType == BackupType.Full;
        set { if (value) SelectedType = BackupType.Full; }
    }

    public bool IsDifferentialBackup
    {
        get => SelectedType == BackupType.Differential;
        set { if (value) SelectedType = BackupType.Differential; }
    }

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

    partial void OnNameChanged(string value) => ConfirmCommand.NotifyCanExecuteChanged();
    partial void OnSourcePathChanged(string value) => ConfirmCommand.NotifyCanExecuteChanged();
    partial void OnTargetPathChanged(string value) => ConfirmCommand.NotifyCanExecuteChanged();

    partial void OnSelectedTypeChanged(BackupType value)
    {
        OnPropertyChanged(nameof(IsFullBackup));
        OnPropertyChanged(nameof(IsDifferentialBackup));
    }

    partial void OnSelectedLogFileTypeChanged(string value)
    {
        OnPropertyChanged(nameof(IsJsonLog));
        OnPropertyChanged(nameof(IsXmlLog));
    }

    // ── Dialog result ─────────────────────────────────────────────────────────

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

    private bool CanConfirm() =>
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(SourcePath) &&
        !string.IsNullOrWhiteSpace(TargetPath);

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        if (HasErrors) return;

        Result = new BackupConfig(Name, SourcePath, TargetPath, SelectedType, SelectedLogFileType, [.. EncryptedExtensions], EncryptKey);
        CloseRequested?.Invoke(true);
    }

    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(false);
}
