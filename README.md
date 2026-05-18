# EasySave — Developer Guide (v3.0)

> This README is intended for code handover. For user documentation, please refer to the separately provided user manual.

---

## Table of Contents

1. [Technical Stack](#1-technical-stack)
2. [Project Structure](#2-project-structure)
3. [Architecture](#3-architecture)
4. [Design Patterns](#4-design-patterns)
5. [Key Execution Flows](#5-key-execution-flows)
6. [Configuration System](#6-configuration-system)
7. [Synchronization and Concurrency](#7-synchronization-and-concurrency)
8. [Logging — EasyLog](#8-logging--easylog)
9. [Coding Conventions](#9-coding-conventions)
10. [Git Workflow](#10-git-workflow)
11. [Extending the Project](#11-extending-the-project)
12. [Build & Launch](#12-build--launch)

---

## 1. Technical Stack

| Element | Choice                                                                               |
|---|-------------------------------------------------------------------------------------|
| Runtime | .NET 10                                                                             |
| UI | [Avalonia](https://avaloniaui.net/) 12.0.1 (cross-platform — Windows, macOS, Linux) |
| MVVM | CommunityToolkit.Mvvm 8.4.1                                                         |
| UI Theme | Avalonia.Themes.Fluent                                                              |
| Serialization | `System.Text.Json` (built-in)                                                       |
| Encryption | External process `CryptoSoft` (see §5)                                              |
| Logging | `EasyLog` — DLL, available as NuGet package                                         |
| Solution format | `.slnx` (new Visual Studio / Rider format)                                          |

**No DI container** — dependency injection is done manually in the factories.
**No database** — all persistence is done through JSON/XML files.
**No automated tests** (to date) — the domain layer is designed to accommodate them.

---

## 2. Project Structure

```
EasySave_Groupe1/
├── src/
│   ├── EasySave.Domain/          # Entities, interfaces, strategies
│   ├── EasySave.Application/     # Orchestration (JobManager, TransferGate)
│   ├── EasySave.Infrastructure/  # Factories, services, subscribers, parsers
│   ├── EasySave.GUI/             # Avalonia UI + ViewModels
│   ├── EasySave.CLI/             # Command line entry point
│   └── EasyLog/                  # Standalone logging library
├── Directory.Build.props         # Shared MSBuild parameters (TFM, nullable, etc.)
├── EasySave.slnx
├── README.md
└── CONTRIBUTING.md
```

### Project dependencies

```
EasySave.GUI  ──────────────────────────────────────┐
EasySave.CLI  ──────────────────────────────────────┤
                                                    ▼
                                       EasySave.Application
                                        ├── EasySave.Domain
                                        └── EasySave.Infrastructure
                                              ├── EasySave.Domain
                                              └── EasyLog
```

The Domain layer has **no external dependencies** — it does not know about the infrastructure, nor the UI.

---

## 3. Architecture

The project follows **Clean Architecture**: dependencies always point inwards (Domain), never outwards.

### 3.1 Domain (`EasySave.Domain`)

Contains the business core, without any dependency on third-party frameworks.

**Main entities:**
- `BackupJob` — a named backup job with its source, destination, strategy, and encryption config
- `BackupConfig` — DTO for creating a job (passed from upper layers to the domain)
- `FileConfig` — deserialized root of `config.json`

**Key interfaces:**

| Interface | Role |
|---|---|
| `IBackupStrategy` | Backup execution contract |
| `IPublisher` | Attaches / detaches `ISubscriber`s |
| `ISubscriber` | Receives an `IBackupEvent` during progression |
| `ITransferGate` | Controls transfer priority and concurrency |
| `IEncryptionService` | File encryption abstraction |
| `ISoftwareDetector` | Detects a blocking process |

**Event models (immutable records):**
```
IBackupEvent
  ├── FileTransferReady    (file about to be copied)
  ├── FileTransferSuccess  (copy successful, includes transfer and encryption duration)
  ├── FileTransferFailure  (exception during copy)
  ├── BackupCompleted      (job completed)
  └── BackupInterrupted    (blocking software detected)
```

**Strategy implementations (also in Domain):**
- `AbstractBackupStrategy` — Template Method + `IPublisher`; handles the copy loop, pause, encryption, and event publication
- `FullBackupStrategy` — copies all files
- `DifferentialBackupStrategy` — copies only files whose `LastWriteTime` is later than the last backup

### 3.2 Application (`EasySave.Application`)

- `JobManager` — complete job lifecycle: addition, concurrent execution (`ConcurrentDictionary<string, Lazy<Task>>`), pause/resume via `ManualResetEvent`
- `TransferGate` — implements `ITransferGate`; two synchronization mechanisms (see §7)

### 3.3 Infrastructure (`EasySave.Infrastructure`)

| Subfolder | Content |
|---|---|
| `Factories/` | `AbstractBackupFactory`, `FullBackupFactory`, `DifferentialBackupFactory` — instantiate jobs and wire up subscribers |
| `Services/` | `CryptoSoftService` (encryption via external process), `SoftwareDetector` (process detection) |
| `Subscribers/` | `StateTracker` (real-time state → `Logs/states.json`), `DailyLogger` (delegates to EasyLog) |
| `Parsers/` | `JsonParser` — deserializes `FileConfig` from a JSON file |

### 3.4 GUI (`EasySave.GUI`)

Avalonia UI in strict MVVM with `CommunityToolkit.Mvvm`.

**Main ViewModels:**

| ViewModel | Role |
|---|---|
| `MainWindowViewModel` | Root — coordinates all tabs |
| `JobsPageViewModel` | Job collection, triggers start / pause / resume / remove |
| `BackupJobViewModel` | Represents a job in the UI; **implements `ISubscriber`** to receive events and update progression |
| `SettingsPageViewModel` | Priority extensions, blocking apps, large file threshold, log format |
| `BackupConfigDialogViewModel` | Form for creating a new job |
| `LogsPageViewModel` | Lists and opens log files |

**ViewLocator** (`ViewLocator.cs`) — automatic convention: removes the `ViewModel` suffix to resolve the corresponding `View` via reflection.

**UI → background thread binding:** all `BackupJobViewModel` updates coming from a worker thread go through `Dispatcher.UIThread.InvokeAsync()`.

### 3.5 CLI (`EasySave.CLI`)

Console entry point. Parses arguments (e.g., `1-3`, `1;3`), loads `config.json`, instantiates jobs via `JobManager`, executes using `Task.WhenAll()`.

---

## 4. Design Patterns

| Pattern | Where | Why |
|---|---|---|
| **Strategy** | `IBackupStrategy` + Full / Differential | Change the backup algorithm without modifying `BackupJob` |
| **Observer** | `IPublisher` / `ISubscriber` + events | Decouple backup execution from logging, state tracking, and UI |
| **Abstract Factory** | `AbstractBackupFactory` + subclasses | Create a complete job (strategy + wired subscribers) in a single call |
| **Template Method** | `AbstractBackupStrategy.Execute()` | Define the skeleton of the copy loop; `GetBackupFiles()` is overridden by subclasses |
| **Singleton** | `EasyLog`, `LocalizationService` | Single shared instance, thread-safe |
| **DTO** | `BackupConfig`, `FileConfig` | Data transport between layers without exposing domain entities |
| **View Locator** | `ViewLocator.cs` | Convention-over-configuration for VM → View mapping in Avalonia |

---

## 5. Key Execution Flows

### Job creation (GUI)

```
BackupConfigDialog (UI)
  └─► BackupConfigDialogViewModel.Confirm()
        └─► MainWindowViewModel.AddBackupConfig(BackupConfig)
              └─► JobsPageViewModel : creates BackupJobViewModel (ISubscriber)
              └─► JobManager.AddJob(config)
                    └─► Factory (Full or Differential)
                          ├─ creates AbstractBackupStrategy
                          ├─ attaches StateTracker
                          ├─ attaches DailyLogger
                          ├─ attaches BackupJobViewModel
                          └─ returns BackupJob
```

### Job execution

```
JobsPageViewModel.RunOrPauseJob()
  └─► JobManager.ExecuteJob(name)          ← returns Task (non-blocking)
        └─► Task.Run(() =>
              BackupJob.Execute(pauseEvent, transferGate)
                └─► AbstractBackupStrategy.Execute()
                      ├─ checks ISoftwareDetector → publishes BackupInterrupted if blocked
                      ├─ GetBackupFiles() → IEnumerable<FileInfo> (lazy)
                      ├─ For each file:
                      │    ├─ pauseEvent.WaitOne()          ← pause point
                      │    ├─ publishes FileTransferReady
                      │    ├─ TransferGate.Acquire()        ← priority / large files
                      │    ├─ File.Copy()
                      │    ├─ CryptoSoftService.Encrypt()   ← if relevant extension
                      │    ├─ TransferGate.Release()
                      │    └─ publishes FileTransferSuccess / Failure
                      └─ publishes BackupCompleted
            )
```

### Encryption via CryptoSoft

`CryptoSoftService` launches the `CryptoSoft` process (or `CryptoSoft.exe` on Windows) with the file path and key as arguments. The process's return code is interpreted as the encryption duration in milliseconds. If the executable is not found, a `FileNotFoundException` is propagated up to the UI as an error dialog.

---

## 6. Configuration System

### `config.json` File

```json
{
  "logFileType": "JSON",
  "logEmplacement": "local",
  "businessSoftwares": ["notepad"],
  "priorityExtensions": [".docx", ".pdf"],
  "largeFileSizeThresholdKb": 100,
  "jobs": [
    {
      "name": "MaSauvegarde",
      "sourcePath": "C:/Source",
      "targetPath": "C:/Target",
      "type": "Full",
      "typesToEncrypt": [".pdf"],
      "encryptKey": "ma_cle"
    }
  ]
}
```

**`type` field**: `"Full"` or `"Differential"` (case-insensitive).
**Encryption**: `typesToEncrypt` and `encryptKey` are optional; if absent, no encryption is applied.
**`logEmplacement`**: `"local"`, `"server"`, or `"both"` — controls where logs are written.

### Loading Flow

- **GUI**: the path is selected via a dialog on startup; `JsonParser.Parse()` deserializes to `FileConfig`, then `JobManager.AddJob()` is called for each entry.
- **CLI**: the path is hardcoded or passed as the first argument; same pipeline `JsonParser → FileConfig → JobManager`.

### `AppSettings` (GUI only)

`AppSettings` is an `ObservableObject` that keeps in memory the settings modifiable from the Settings tab. Changes are persisted to `config.json` upon closing or on demand.

---

## 7. Synchronization and Concurrency

Two distinct mechanisms coexist in `TransferGate`:

### Extension Priority

Files whose extension is in `priorityExtensions` are processed **before** all others.

- The strategy sorts files and counts priority files.
- Non-priority files call `_priorityGate.Wait()` (`ManualResetEventSlim`).
- When the last priority file is processed, `_priorityGate.Set()` unblocks the others.

### Large File Limitation

Only one file exceeding `largeFileSizeThresholdKb` can be transferred at a time.

- `_largeFileLock` is a `SemaphoreSlim(1, 1)`.
- Any large file calls `_largeFileLock.Wait()` before copying and `_largeFileLock.Release()` afterwards.

### Pause / Resume

Each job has a dedicated `ManualResetEvent` (initially signaled = `true`).
- `JobManager.PauseJob(name)` → `pauseEvent.Reset()` — blocks at the next `WaitOne()` in the copy loop.
- `JobManager.ResumeJob(name)` → `pauseEvent.Set()` — resumes.

---

## 8. Logging — EasyLog

`EasyLog` is a standalone library (no dependency on other EasySave projects).

- **Singleton** `EasyLog` — single entry point.
- **Strategy** (internal): `JsonLogger` (JSONL — one JSON entry per line) or `XmlLogger`.
- The format is chosen by `logFileType` in `config.json`.
- Files are named by date and written in `Logs/`.
- `StateTracker` writes `Logs/states.json` in real-time using atomic writing (write → `.tmp` → rename).

---

## 9. Coding Conventions

See `CONTRIBUTING.md` for full details. Summary of key points:

### C# Naming

| Target | Convention | Example |
|---|---|---|
| Classes, public methods, public properties | PascalCase | `JobManager`, `Execute()` |
| Local variables, parameters | camelCase | `jobName`, `pauseEvent` |
| Private fields | _camelCase | `_subscribers`, `_jobManager` |
| Interfaces | IPascalCase | `IBackupStrategy` |
| Enums | PascalCase (singular) | `BackupType` |
| Events / event records | `[Action][Result]` | `FileTransferSuccess` |

### Avalonia / MVVM Specifics

- Observable properties use `[ObservableProperty]` (CommunityToolkit) — the backing field is generated automatically.
- UI commands use `[RelayCommand]`.
- ViewModels that require validation inherit from `ObservableValidator`.
- Bindings are **compiled** (`AvaloniaUseCompiledBindingsByDefault=true`) — binding errors are detected at compile time.

### General Style

- Records for immutable data (events, metadata).
- `init` accessors for properties settable only at construction.
- Pattern matching preferred over explicit casts.
- `Directory.EnumerateFiles` (lazy) rather than `GetFiles` (eager).
- Case-insensitive extension comparison: `StringComparer.OrdinalIgnoreCase`.
- The code documents *how*, comments document *why*.

---

## 10. Git Workflow

```
main          ← stable releases only
  └── dev     ← main integration branch
        └── feat/ma-feature   ← always branch from dev
        └── fix/mon-bug
        └── refactor/...
```

- **Commits**: [Conventional Commits](https://www.conventionalcommits.org/) — `feat:`, `fix:`, `refactor:`, `chore:`, `docs:`
- **Branches**: `type/short-description` in kebab-case
- **Versioning**: [SemVer](https://semver.org/) — `Major.Minor.Patch`

---

## 11. Extending the Project

### New Backup Strategy

1. Create a class in `EasySave.Domain/Strategies/` inheriting from `AbstractBackupStrategy`.
2. Override `GetBackupFiles(string sourcePath)` to return the files to copy.
3. Add a value to the `BackupType` enum.
4. Create a factory in `EasySave.Infrastructure/Factories/` inheriting from `AbstractBackupFactory`.
5. Wire the factory in `JobManager.AddJob()`.

### New Subscriber (logger, notifier, etc.)

1. Implement `ISubscriber` with `Update(IBackupEvent e)` in `EasySave.Infrastructure/Subscribers/`.
2. Register it in the relevant factory via `Attach(subscriber)`.

### New Log Format

1. Create a class in `EasyLog/` inheriting from `AbstractLogger`.
2. Implement `Write(string entry)`.
3. Wire into `EasyLog` according to the `logFileType` value.

### New Screen (GUI)

1. Create a ViewModel in `EasySave.GUI/ViewModels/` inheriting from `ObservableObject` (or `ObservableValidator` if validation is needed).
2. Create the corresponding `.axaml` View in `EasySave.GUI/Views/` — the `ViewLocator` resolves it automatically.
3. Expose the ViewModel from `MainWindowViewModel`.

---

## 12. Build & Launch

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `CryptoSoft` (or `CryptoSoft.exe` on Windows) accessible in the `PATH` for encryption features

### Build

```bash
dotnet build EasySave.slnx
```

### Launch GUI

```bash
dotnet run --project src/EasySave.GUI
```

### Launch CLI

```bash
# Single job
dotnet run --project src/EasySave.CLI 1

# Job range (1 to 3)
dotnet run --project src/EasySave.CLI 1-3

# Non-consecutive jobs
dotnet run --project src/EasySave.CLI 1;3
```

The numbers correspond to the declaration order of the jobs in `config.json`.

### Publish (standalone binary)

```bash
dotnet publish src/EasySave.GUI -c Release -r win-x64
dotnet publish src/EasySave.CLI -c Release -r linux-x64
```

`Directory.Build.props` configures publication as **single-file self-contained** by default.