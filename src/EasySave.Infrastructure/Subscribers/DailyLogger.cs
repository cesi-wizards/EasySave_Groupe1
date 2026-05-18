using EasySave.Domain.Interfaces;
using EasySave.Domain.Events;

namespace EasySave.Infrastructure.Subscribers;

public class DailyLogger : ISubscriber
{
    private const string ServerAddress = "localhost";
    private const int    ServerPort    = 11000;

    private readonly string _logFileType;
    private readonly string _logEmplacement;

    public DailyLogger(string logFileType, string logEmplacement = "local")
    {
        _logFileType    = logFileType;
        _logEmplacement = logEmplacement;
    }

    private string GetLogFilePath()
    {
        string folderName = "Logs";
        string fileName = $"{DateTime.Now:yyyy-MM-dd}";

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(appData, "EasySave", folderName, fileName);
    }

    public void Update(IBackupEvent backupEvent)
    {
        switch (backupEvent)
        {
            case FileTransferSuccess e:
                WriteToFile(Serialize(e.Meta, e.File, e.TransferTime, e.EncryptTime, null));
                break;
            case FileTransferFailure e:
                WriteToFile(Serialize(e.Meta, e.File, TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(-1), e.Reason));
                break;
            case BackupInterrupted e:
                WriteToFile(Serialize(e.Meta, null, TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(-1), e.Reason));
                break;
        }
    }


    private Dictionary<string, object> Serialize
        (
            EventMetadata meta,
            BackupFileInfo? file, TimeSpan transferTime, TimeSpan encryptTime,
            string? errorMessage
        )
    {
        return new Dictionary<string, object>()
        {
            { "DateJob", DateTimeOffset.FromUnixTimeMilliseconds(meta.Timestamp).DateTime.ToString("yyyy-MM-dd HH:mm:ss") },
            { "JobName", meta.JobName },
            { "SourcePath", file?.SourcePath ?? string.Empty },
            { "TargetPath", file?.TargetPath ?? string.Empty },
            { "FileSize", file?.FileSize ?? 0 },
            { "TransfertTime", transferTime },
            { "EncryptTime", encryptTime },
            { "ErrorMessage", errorMessage ?? string.Empty }
        };
    }

    private void WriteToFile(Dictionary<string, object> content)
    {
        string filePath   = _logEmplacement is "local" or "both" ? GetLogFilePath() : string.Empty;
        string serverName = _logEmplacement is "server" or "both" ? ServerAddress    : string.Empty;
        int    port       = _logEmplacement is "server" or "both" ? ServerPort        : 0;
        EasyLog.EasyLog.Instance.Write(filePath, content, _logFileType, serverName, port);
    }
}
