using System.Text.Json;
using EasySave.Domain.Entities;
using EasySave.Domain.Enum;
using EasySave.Domain.Interfaces;

namespace EasySave.Infrastructure.Subscribers;

public class StateTracker : ISubscriber
{
    private static readonly object _fileBlock = new object();

    private string GetStatePath()
    {
        string folderName = "Logs";
        string fileName = "states.json";

        // ===== CHEMIN LOCAL =====
        string local = Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);

        return local;
        // ===== CHEMIN LOCAL =====

        // ===== CHEMIN UNC =====
        /*
            string serveur = "";
            string unc = &@"\\{serveur}\{folderName}\{folderName}";
            return unc;
        */
        // ===== CHEMIN UNC ====
    }

    public void Update(Context context)
    {
        WriteToFile(Serialize(context));
    }

    private Dictionary<string, object> Serialize(Context context)
    {
        if(context.RemainingCount == 0)
        {
            return new Dictionary<string, object>()
            {
                {"JobName", context.JobName },
                {"DateJob", DateTimeOffset.FromUnixTimeSeconds(context.Timestamp).DateTime.ToString("yyyy-MM-dd HH:mm:ss")},
                {"State", ActiveSaveStateEnumExtensions.GetStateLabel(ActiveSaveStateEnum.INACTIVE) },

                {"TotalFileCount", 0 },
                {"TotalFileSize", 0 },
                {"Progression", "0" },
                {"RemainingFileCount", 0 },
                {"RemainingFileSize", 0},
                {"SourcePath", string.Empty },
                {"TargetPath", string.Empty }
            };
        }

        int actualFile = context.TotalCount - context.RemainingCount;
        int percent = context.TotalCount > 0
            ? (int)((double)actualFile / context.TotalCount * 100)
            : 0;

        string progression = $"{actualFile}/{context.TotalCount} - ({percent}%)";

        return new Dictionary<string, object>()
        {
            {"JobName", context.JobName },
            {"DateJob", DateTimeOffset.FromUnixTimeSeconds(context.Timestamp).DateTime.ToString("yyyy-MM-dd HH:mm:ss") },
            {"State", ActiveSaveStateEnumExtensions.GetStateLabel(ActiveSaveStateEnum.ACTIVE) },

            {"TotalFileCount", context.TotalCount },
            {"TotalFileSize", context.TotalSize },
            {"Progression", progression },
            {"RemainingFileCount", context.RemainingCount },
            {"RemainingFileSize", context.RemainingSize },
            {"SourcePath", context.SourcePath },
            {"TargetPath", context.TargetPath }
        };
    }

    private string ListDictionaryToJson(List<Dictionary<string, object>> infoContext)
    {
        try
        {
            // options for the serialization
            var options = new JsonSerializerOptions
            {
                WriteIndented = true, // pretty-prints the JSON
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

    private void EnsureDirectoryExists(string statePath)
    {
        try
        {
            string? directory = Path.GetDirectoryName(statePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"Permission denied when creating the folder: {statePath}", ex);
        }
        catch (PathTooLongException ex)
        {
            throw new IOException($"File path is too long for the file tree: {statePath}", ex);
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
                    string fileData = File.ReadAllText(GetStatePath());
                    states = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(fileData) ?? new List<Dictionary<string, object>>();
                }
                else
                {
                    states = new List<Dictionary<string, object>>();
                }

                Dictionary<string, object>? item = states.FirstOrDefault(state => state.ContainsKey("JobName") && state["JobName"].ToString() == infoContext["JobName"].ToString());

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

                string updatedJson = ListDictionaryToJson(states);

                string statePath = GetStatePath();
                EnsureDirectoryExists(statePath);
                File.WriteAllText(statePath, updatedJson);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to write the State : {GetStatePath()}", ex);
            }
        }
    }
}
