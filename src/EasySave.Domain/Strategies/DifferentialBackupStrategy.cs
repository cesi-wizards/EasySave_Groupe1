using System.Diagnostics;

namespace EasySave.Domain.Strategies;

public class DifferentialBackupStrategy : AbstractBackupStrategy
{
    public override void Execute(string sourcePath, string targetPath)
    {
        foreach (var sourceFile in GetFiles(sourcePath))
        {
            var relativePath = Path.GetRelativePath(sourcePath, sourceFile);
            var targetFile = Path.Combine(targetPath, relativePath);

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
                CopyFile(sourceFile, targetFile);
            }
        }
    }
}
