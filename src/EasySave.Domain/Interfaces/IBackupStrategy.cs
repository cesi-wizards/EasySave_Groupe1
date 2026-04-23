namespace EasySave.Domain.Interfaces;

public interface IBackupStrategy
{
    void Execute(string jobName, string sourcePath, string targetPath);
}
