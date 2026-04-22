using System.Reflection.Metadata.Ecma335;
using EasyLog;

namespace EasySave.Infrastructure.Subscribers;

public class DailyLogger
{
    private string GetLogFilePath()
    {
        string folderName = "logs";
        string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";

        // ===== CHEMIN LOCAL =====
        string local = &@"\{folderName}\{folderName}";

        return local;
        // ===== CHEMIN LOCAL =====

        // ===== CHEMIN UNC =====
        /*
            string serveur = "";
            string UNC = &@"\\{serveur}\{folderName}\{folderName}";
            return UNC;
        */
        // ===== CHEMIN UNC ====
    }

    public void Update(Context context)
    {
        if (context.TransferTime > 0)
        {
            WriteToFile(Serialize(context));
        }
    }

    private Dictionary<string, object> Serialize(Context context)
    {
        Dictionary<string, object> infoContext = new Dictionary<string, object>()
        {
            {"DateJob", context.DateJob.ToString("yyyy-MM-dd HH:mm::ss")},
            {"JobName", context.JobName },
            {"SourcePath", context.SourcePath },
            {"TargetPath", context.TargetPath },
            {"FileSize", context.FileSize },
            {"TransfertTime", context.TransferTime }
        };

        return infoContext;
    }

    private void WriteToFile(Dictionary<string, object> infoContext)
    {
        EasyLog.EasyLog.Instance.LogJson(GetLogFilePath(), Serialize(infoContext));
    }
}
