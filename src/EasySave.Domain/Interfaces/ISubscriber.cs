using EasySave.Domain.Events;

namespace EasySave.Domain.Interfaces;

public interface ISubscriber
{
    void Update(IBackupEvent backupEvent);
}
