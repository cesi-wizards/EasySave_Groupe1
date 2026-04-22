namespace EasySave.Domain.Strategies;

public class FullBackupStrategy : AbstractBackupStrategy
{
    public override void Execute(string sourcePath, string targetPath)
    {
        foreach (var sourceFile in GetFiles(sourcePath))
        {
            var relativePath = Path.GetRelativePath(sourcePath, sourceFile);
            var targetFile = Path.Combine(targetPath, relativePath);
            CopyFile(sourceFile, targetFile);
        }
    }
}
