namespace EasySave.Domain.Enum;

public enum ActiveSaveStateEnum
{
    // Attributs
    ACTIVE,
    INACTIVE,
}

public static class ActiveSaveStateEnumExtensions
{
    public static string GetStateLabel(ActiveSaveStateEnum state){
        return state switch
        {
            ActiveSaveStateEnum.ACTIVE => "Running",
            ActiveSaveStateEnum.INACTIVE => "Not running",
            _ => "Unknown"
        };
    }
}

