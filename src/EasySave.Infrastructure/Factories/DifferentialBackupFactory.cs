using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Strategies;
using EasySave.Infrastructure.Services;

namespace EasySave.Infrastructure.Factories;

public class DifferentialBackupFactory(List<ISubscriber> subscribers, ISoftwareDetector? softwareDetector = null)
    : AbstractBackupFactory(subscribers, softwareDetector)
{
    public override BackupJob CreateJob(string jobName, string sourcePath, string targetPath, List<string> TypesToEncrypt, string encryptKey)
    {
        AbstractBackupStrategy strategy = new DifferentialBackupStrategy(new CryptoSoftService(), SoftwareDetector);
        return CreateJobWithStrategy(jobName, sourcePath, targetPath, strategy, TypesToEncrypt, encryptKey);
    }
}
