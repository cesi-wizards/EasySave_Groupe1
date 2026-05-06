namespace EasySave.Domain.Events;

public record FileTransferReady
(
    EventMetadata Meta,
    BackupFileInfo File,
    BackupProgress Progress
)
: IBackupEvent;

public record FileTransferSuccess
(
    EventMetadata Meta,
    BackupFileInfo File,
    TimeSpan TransferTime,
    TimeSpan EncryptTime,
    BackupProgress Progress
)
: IBackupEvent;

public record FileTransferFailure
(
    EventMetadata Meta,
    BackupFileInfo File,
    string Reason,
    BackupProgress Progress
)
: IBackupEvent;

public record BackupInterrupted
(
    EventMetadata Meta,
    string Reason
)
: IBackupEvent;
