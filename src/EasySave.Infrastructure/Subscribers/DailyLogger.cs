using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;

namespace EasySave.Infrastructure.Subscribers;

public class DailyLogger : ISubscriber
{
    private string GetLogFilePath()
    {
        string folderName = "Logs";
        string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";

        // ===== CHEMIN LOCAL =====
        string local = Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);

        return local;
        // ===== /CHEMIN LOCAL =====

        // ===== CHEMIN UNC =====

        //string serveur = "";
        //string unc = $@"\\{serveur}\{folderName}\{folderName}";
        //return unc;

        // ===== /CHEMIN UNC ====
    }

    public void Update(Context context)
    {
        if (context.TransferTime != TimeSpan.Zero)
        {
            WriteToFile(context);
        }
    }


    private Dictionary<string, object> Serialize(Context context)
    {
        return new Dictionary<string, object>()
        {
            { "DateJob", context.Timestamp.ToString("yyyy-MM-dd HH:mm:ss") },
            { "JobName", context.JobName },
            { "SourcePath", context.SourcePath },
            { "TargetPath", context.TargetPath },
            { "FileSize", context.FileSize },
            { "TransfertTime", context.TransferTime }
        };
    }

private void WriteToFile(Context context)
    {
        EasyLog.EasyLog.Instance.LogJson(GetLogFilePath(), Serialize(context));
    }
}
