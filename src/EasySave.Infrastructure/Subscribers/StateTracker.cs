using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using EasySave.Domain.Entities;

namespace EasySave.Infrastructure.Subscribers;

public class StateTracker
{
    private static readonly object _fileBlock = new object();

    private string GetStatePath()
    {
        string folderName = "logs";
        string fileName = $"states.json";

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
        WriteToFile(Serialize(context));
    }

    private Dictionary<string, object> Serialize(Context context)
    {
        if(context.SourcePath == null && context.TargetPath == null)
        {
            Dictionary<string, object> infoContext = new Dictionary<string, object>()
            {
                {"JobName", context.JobName },
                {"DateJob", context.DateJob },
                {"State", ActiveSaveStateEnum.GetStateLabel(ActiveSaveStateEnum.INACTIVE) },

                {"TotalFileCount", 0 },
                {"TotalFileSize", 0 },
                {"Progression", "0" },
                {"RemainingFileCount", 0 },
                {"RemainingFileSize", 0},
                {"SourcePath", string.Empty },
                {"TargetPath", string.Empty }
            };

            return infoContext;
        }

        int actualFile = context.TotalCount - context.RemainingCount;
        int percent = (context.RemainingCount / context.TotalCount) * 100;

        string progression = &@"{actualFile}/{context.TotalCount} - ({percent}%)";

        Dictionary<string, object> infoContext = new Dictionary<string, object>()
        {
            {"JobName", context.JobName },
            {"DateJob", context.DateJob },
            {"State", ActiveSaveStateEnum.GetStateLabel(ActiveSaveStateEnum.ACTIVE) },

            {"TotalFileCount", context.TotalCount },
            {"TotalFileSize", context.TotalSize },
            {"Progression", progression },
            {"RemainingFileCount", context.RemainingCount },
            {"RemainingFileSize", context.RemainingSize },
            {"SourcePath", context.SourcePath },
            {"TargetPath", context.TargetPath }
        };

        return infoContext;
    }

    private string ListDictionaryToJSON(List<Dictionary<string, object>> infoContext)
    {
        try
        {
            // options for the serialization
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // writes all un a single line
                PropertyNamingPolicy = null // keep the name of the keys
            };

            return JsonSerializer.Serialize(infoContext, options);
        }
        catch (Exception)
        {
            // In case of an exception
            return "{}";
        }
    }

    private void WriteToFile(Dictionary<string, object> infoContext)
    {
        lock (_fileBlock)
        {
            List<Dictionary<string, object>> states;

            try
            {
                if (File.Exists(GetStatePath()))
                {
                    string fileData = File.ReadAllText(filePath);
                    states = JsonSerializer.Deserialize < List < Dictionary<string, object> >> ?? new List<Dictionary<string, object>>();
                }
                else
                {
                    states = new List<Dictionary<string, object>>();
                }

                var item = states.FirstOrDefault(state => state.ContainsKey("JobName") && state["saveName"].ToString() == infoContext["JobName"].ToString);

                if(item != null)
                {
                    // mapping
                    item["JobName"] = infoContext["JobName"];
                    item["DateJob"] = infoContext["DateJob"];
                    item["State"] = infoContext["State"];
                    item["TotalFileCount"] = infoContext["TotalFileCount"];
                    item["TotalFileSize"] = infoContext["TotalFileSize"];
                    item["Progression"] = infoContext["Progression"];
                    item["RemainingFileCount"] = infoContext["RemainingFileCount"];
                    item["RemainingFileSize"] = infoContext["RemainingFileSize"];
                    item["SourcePath"] = infoContext["SourcePath"];
                    item["TargetPath"] = infoContext["TargetPath"];
                }
                else
                {
                    states.Add(infoContext);
                }

                string updatedJson = ListDirectoryToJSON(infoContext);
                File.WriteAllText(GetStatePath(), updatedJson);
            }
            catch (Exception ex)
            {
            }
        }

    }
}
