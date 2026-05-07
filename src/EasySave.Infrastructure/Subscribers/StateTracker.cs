using System.Text.Json;
using System.Linq;
using EasySave.Domain.Interfaces;
using EasySave.Domain.Enum;
using EasySave.Domain.Events;

namespace EasySave.Infrastructure.Subscribers;

public class StateTracker : ISubscriber
{
    private static readonly object _fileBlock = new object();

    private string GetStatePath()
    {
        string folderName = "Logs";
        string fileName = "states.json";

        return Path.Combine(Directory.GetCurrentDirectory(), folderName, fileName);
    }

    public void Update(IBackupEvent backupEvent)
    {
        switch (backupEvent)
        {
            case FileTransferReady e:
                WriteToFile(Serialize(e.Meta, e.Progress, e.File, ActiveSaveStateEnum.ACTIVE));
                break;
            case FileTransferSuccess e:
                WriteToFile(Serialize(e.Meta, e.Progress, e.File, ActiveSaveStateEnum.ACTIVE));
                break;
            case FileTransferFailure e:
                WriteToFile(Serialize(e.Meta, e.Progress, e.File, ActiveSaveStateEnum.ACTIVE));
                break;
            case BackupInterrupted e:
                WriteToFile(Serialize(e.Meta, null, null, ActiveSaveStateEnum.INACTIVE));
                break;
            case BackupCompleted e:
                WriteToFile(new Dictionary<string, object>
                {
                    {"JobName", e.Meta.JobName},
                    {"DateJob", DateTimeOffset.FromUnixTimeMilliseconds(e.Meta.Timestamp).DateTime.ToString("yyyy-MM-dd HH:mm:ss")},
                    {"State", ActiveSaveStateEnumExtensions.GetStateLabel(ActiveSaveStateEnum.INACTIVE)},
                    {"TotalFileCount", e.TotalFiles},
                    {"TotalFileSize", e.TotalSize},
                    {"Progression", $"{e.TotalFiles}/{e.TotalFiles} - (100%)"},
                    {"RemainingFileCount", 0},
                    {"RemainingFileSize", 0},
                    {"SourcePath", string.Empty},
                    {"TargetPath", string.Empty}
                });
                break;
        }
    }

    private Dictionary<string, object> Serialize
        (
            EventMetadata meta,
            BackupProgress? progress,
            BackupFileInfo? file,
            ActiveSaveStateEnum state
        )
    {
        int totalCount = progress?.TotalCount ?? 0;
        int remainingCount = progress?.RemainingCount ?? 0;
        long totalSize = progress?.TotalSize ?? 0;
        long remainingSize = progress?.RemainingSize ?? 0;

        int actualFile = totalCount - remainingCount;
        int percent = totalCount > 0 ? (int)((double)actualFile / totalCount * 100) : 0;
        string progression = $"{actualFile}/{totalCount} - ({percent}%)";

        return new Dictionary<string, object>()
        {
            {"JobName", meta.JobName },
            {"DateJob", DateTimeOffset.FromUnixTimeMilliseconds(meta.Timestamp).DateTime.ToString("yyyy-MM-dd HH:mm:ss") },
            {"State", ActiveSaveStateEnumExtensions.GetStateLabel(state) },

            {"TotalFileCount", totalCount },
            {"TotalFileSize", totalSize },
            {"Progression", progression },
            {"RemainingFileCount", remainingCount },
            {"RemainingFileSize", remainingSize },
            {"SourcePath", file?.SourcePath ?? string.Empty },
            {"TargetPath", file?.TargetPath ?? string.Empty }
        };
    }

    private string ListDictionaryToJson(List<Dictionary<string, object>> infoContext)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = null
            };

            return JsonSerializer.Serialize(infoContext, options);
        }
        catch (Exception)
        {
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

                if (item != null)
                {
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
