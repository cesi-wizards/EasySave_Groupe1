using System.Diagnostics;

using EasySave.Domain.Interfaces;

namespace EasySave.Infrastructure.Services;

public class SoftwareDetector : ISoftwareDetector
{
    private HashSet<string> _processNames;

    public SoftwareDetector(IEnumerable<string> processNames)
    {
        _processNames = processNames
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => Path
            .GetFileNameWithoutExtension(p).Trim().ToLowerInvariant())
            .ToHashSet();
    }

    public void UpdateProcessNames(IEnumerable<string> processNames)
    {
        _processNames = processNames
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => Path.GetFileNameWithoutExtension(p).Trim().ToLowerInvariant())
            .ToHashSet();
    }

    public bool IsSoftwareRunning()
    {
        Console.WriteLine(_processNames.Count);
        if (_processNames.Count == 0)
        {
            return false;
        }
        Console.WriteLine(Process.GetProcessesByName(_processNames.First()));
        foreach (string name in _processNames)
        {
            if (Process.GetProcessesByName(name).Length > 0)
            {
                return true;
            }
        }

        return false;
    }
}
