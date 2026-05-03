namespace EasySave.Domain.Interfaces;

public interface ISoftwareDetector
{
    bool IsSoftwareRunning();
    void UpdateProcessNames(IEnumerable<string> processNames);
}
