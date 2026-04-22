using EasySave.Domain.Entities;
using EasySave.Domain.Interfaces;

namespace EasySave.Infrastructure.Subscribers;

public class DailyLogger : ISubscriber
{
    private string GetLogFilePath()
    {
        string folderName = "logs";
        string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";

        // ===== CHEMIN LOCAL =====
        string local = $@"\{folderName}\{fileName}";

        return local;
        // ===== /CHEMIN LOCAL =====

        // ===== CHEMIN UNC =====

        string serveur = "";
        string unc = $@"\\{serveur}\{folderName}\{folderName}";
        //return unc;

        // ===== /CHEMIN UNC ====
    }

    public void Update(Context context)
    {
        if (context.TransferTime != 0)
        {
            WriteToFile(context);
        }
    }


    private Dictionary<string, object> Serialize(Context context)
    {
        // Positive transfertime -> normal object
        if (context.TransferTime < 0)
        {
            return new Dictionary<string, object>()
            {
                { "DateJob", context.DateJob.ToString("yyyy-MM-dd HH:mm:ss") },
                { "JobName", context.JobName },
                { "SourcePath", context.SourcePath },
                { "TargetPath", context.TargetPath },
                { "FileSize", context.FileSize },
                { "TransfertTime", context.TransferTime }
            };
        }
        // else negative transfertime -> Must write an error
        else
        {
            return new Dictionary<string, object>() { { "DateError", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") } };
        }
    }

private void WriteToFile(Context context)
    {
        EasyLog.EasyLog.Instance.LogJson(GetLogFilePath(), Serialize(context));
    }
}
