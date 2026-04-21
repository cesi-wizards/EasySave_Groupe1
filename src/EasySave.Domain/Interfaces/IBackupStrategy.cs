namespace EasySave.Domain.Interfaces;

public interface IBackupStrategy
{
    void Execute(string sourcePath, string targetPath);
}
