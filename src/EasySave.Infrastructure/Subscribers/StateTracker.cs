using System.Reflection;
using System.Runtime.Serialization.Json;
using EasySave.Domain.Entities;

namespace EasySave.Infrastructure.Subscribers;

public class StateTracker
{
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
                {"State", ActiveSaveStateEnum.GetStateLabel(ActiveSaveStateEnum.INACTIVE) }
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

    private void WriteToFile()
    {

    }
}
