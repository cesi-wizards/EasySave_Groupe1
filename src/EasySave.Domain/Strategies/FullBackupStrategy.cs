using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Strategies;

public class FullBackupStrategy(IEncryptionService encryptionService)
    : AbstractBackupStrategy(encryptionService)
{
    public FullBackupStrategy(ISoftwareDetector? softwareDetector = null)
    : base(softwareDetector) { }

    protected override (List<string>, int, long) GetFilesToBackup(string sourcePath, string targetPath)
    {
        var toBackup = new List<string>();
        int count = 0; long size = 0;

        foreach (string sourceFile in GetFiles(sourcePath))
        {
            toBackup.Add(sourceFile);
            count++;
            size += new FileInfo(sourceFile).Length;
        }
        return (toBackup, count, size);
    }
}
