namespace EasySave.Domain.Interfaces;

public interface IBackupStrategy
{
    void Execute(string JobName, string sourcePath, string targetPath);
}
