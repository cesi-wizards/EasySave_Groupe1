namespace EasySave.Domain.Events;

public interface IBackupEvent
{
    EventMetadata Meta { get; }
}
