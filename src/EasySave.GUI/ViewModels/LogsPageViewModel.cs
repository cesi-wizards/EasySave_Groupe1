using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace EasySave.GUI.ViewModels;

public partial class LogsPageViewModel : ViewModelBase
{
    private static readonly string LogsFolder =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EasySave", "Logs");

    public ObservableCollection<LogFileItem> Files { get; } = [];

    public bool HasFiles  => Files.Count > 0;
    public bool HasNoFiles => Files.Count == 0;

    public void Refresh()
    {
        Files.Clear();

        if (!Directory.Exists(LogsFolder))
            return;

        foreach (var path in Directory.EnumerateFiles(LogsFolder)
                                      .OrderByDescending(File.GetLastWriteTime))
        {
            if (!path.EndsWith("states.json"))
            {
                Files.Add(new LogFileItem(path));
            }
        }

        OnPropertyChanged(nameof(HasFiles));
        OnPropertyChanged(nameof(HasNoFiles));
    }

    [RelayCommand]
    private void OpenFile(LogFileItem item)
    {
        if (OperatingSystem.IsMacOS())
            Process.Start("open", ["-t", item.FullPath]);
        else if (OperatingSystem.IsWindows())
            Process.Start("notepad.exe", item.FullPath);
        else
            Process.Start(new ProcessStartInfo(item.FullPath) { UseShellExecute = true });
    }
}

public class LogFileItem(string fullPath)
{
    public string FullPath { get; } = fullPath;
    public string Name     { get; } = Path.GetFileName(fullPath);
    public string Size     { get; } = FormatSize(new FileInfo(fullPath).Length);
    public string Date     { get; } = File.GetLastWriteTime(fullPath).ToString("yyyy-MM-dd HH:mm");

    private static string FormatSize(long bytes) => bytes switch
    {
        < 1024        => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        _             => $"{bytes / (1024.0 * 1024):F1} MB",
    };
}
