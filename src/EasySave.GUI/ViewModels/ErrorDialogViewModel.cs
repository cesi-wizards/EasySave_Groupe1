using System;
using System.Linq;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.Input;

namespace EasySave.GUI.ViewModels;

public partial class ErrorDialogViewModel : ViewModelBase
{
    public string Title { get; }
    public string Message { get; }
    public string? Details { get; }
    public bool HasDetails => !string.IsNullOrWhiteSpace(Details);
    public string? ActionUrl { get; }
    public bool HasActionUrl => ActionUrl is not null;

    public event Action? CloseRequested;

    public ErrorDialogViewModel(Exception ex)
    {
        Title = ex.GetType().Name;
        Details = ex.StackTrace;

        var urlMatch = Regex.Match(ex.Message, @"https?://\S+");
        if (urlMatch.Success)
        {
            Message = ex.Message[..urlMatch.Index].TrimEnd();
            ActionUrl = urlMatch.Value;
        }
        else
        {
            Message = ex.Message;
        }
    }

    [RelayCommand]
    private void Close() => CloseRequested?.Invoke();
}
