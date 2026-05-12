using System;
using CommunityToolkit.Mvvm.Input;

namespace EasySave.GUI.ViewModels;

public partial class ErrorDialogViewModel : ViewModelBase
{
    public string Title { get; }
    public string Message { get; }
    public string? Details { get; }
    public bool HasDetails => !string.IsNullOrWhiteSpace(Details);

    public event Action? CloseRequested;

    public ErrorDialogViewModel(Exception ex)
    {
        Title = ex.GetType().Name;
        Message = ex.Message;
        Details = ex.StackTrace;
    }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke();
}