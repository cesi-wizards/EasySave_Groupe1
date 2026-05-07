using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Strategies;

public class FullBackupStrategy(IEncryptionService encryptionService, ISoftwareDetector? softwareDetector = null)
    : AbstractBackupStrategy(encryptionService, softwareDetector)
{

    protected override (List<string>, int, long) GetBackupFiles(string sourcePath, string targetPath)
    {
        var toBackup = new List<string>();
        int count = 0; long size = 0;

        foreach (string sourceFile in GetAllFiles(sourcePath))
        {
            toBackup.Add(sourceFile);
            count++;
            size += new FileInfo(sourceFile).Length;
        }
        return (toBackup, count, size);
    }
}
