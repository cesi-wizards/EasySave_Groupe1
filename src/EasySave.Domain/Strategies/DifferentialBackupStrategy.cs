using System.Diagnostics;

using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;

namespace EasySave.Domain.Strategies;

public class DifferentialBackupStrategy(IEncryptionService encryptionService, ISoftwareDetector? softwareDetector = null)
    : AbstractBackupStrategy(encryptionService, softwareDetector)
{
    protected override (List<string>, int, long) GetBackupFiles(string sourcePath, string targetPath)
    {
        var toBackup = new List<string>();
        int count = 0; long size = 0;

        foreach (string sourceFile in GetAllFiles(sourcePath)) // GetFiles() returns a generator so it won't load unused files into memory
        {
            string targetFile = GetTargetFile(sourcePath, targetPath, sourceFile);

            bool needsBackup = true;
            if (File.Exists(targetFile))
            {
                var sourceLastWrite = File.GetLastWriteTime(sourceFile);
                var targetLastWrite = File.GetLastWriteTime(targetFile);
                if (targetLastWrite >= sourceLastWrite)
                {
                    needsBackup = false;
                }
            }
            if (needsBackup)
            {
                toBackup.Add(sourceFile);
                count++;
                size += new FileInfo(sourceFile).Length;
            }
        }
        return (toBackup, count, size);
    }
}
