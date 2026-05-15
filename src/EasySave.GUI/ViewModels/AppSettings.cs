using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;

namespace EasySave.GUI.ViewModels;

public partial class AppSettings : ObservableObject
{
    [ObservableProperty] private string _logFileType = "JSON";
    [ObservableProperty] private string _blockingApp = string.Empty;
    [ObservableProperty] private string _logEmplacement = "local";
    [ObservableProperty] private string _largeFileSizeThresholdKb = "0";
    public ObservableCollection<string> PriorityExtensions { get; } = [];
}
