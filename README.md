# EasySave — Guide développeur (v3.0)

> Ce README est destiné à la reprise du code. Pour la documentation utilisateur, référez-vous au manuel utilisateur fourni séparément.

---

## Table des matières

1. [Stack technique](#1-stack-technique)
2. [Structure du projet](#2-structure-du-projet)
3. [Architecture](#3-architecture)
4. [Patterns de conception](#4-patterns-de-conception)
5. [Flux d'exécution clés](#5-flux-dexécution-clés)
6. [Système de configuration](#6-système-de-configuration)
7. [Synchronisation et concurrence](#7-synchronisation-et-concurrence)
8. [Journalisation — EasyLog](#8-journalisation--easylog)
9. [Conventions de code](#9-conventions-de-code)
10. [Workflow Git](#10-workflow-git)
11. [Étendre le projet](#11-étendre-le-projet)
12. [Build & lancement](#12-build--lancement)

---

## 1. Stack technique

| Élément | Choix                                                                               |
|---|-------------------------------------------------------------------------------------|
| Runtime | .NET 10                                                                             |
| UI | [Avalonia](https://avaloniaui.net/) 12.0.1 (cross-platform — Windows, macOS, Linux) |
| MVVM | CommunityToolkit.Mvvm 8.4.1                                                         |
| Thème UI | Avalonia.Themes.Fluent                                                              |
| Sérialisation | `System.Text.Json` (intégré)                                                        |
| Chiffrement | Processus externe `CryptoSoft` (voir §5)                                            |
| Logging | `EasyLog` — DLL, disponible en package NuGet                                        |
| Format solution | `.slnx` (nouveau format Visual Studio / Rider)                                      |

**Pas de DI container** — l'injection de dépendances est faite manuellement dans les factories.
**Pas de base de données** — toute la persistance passe par des fichiers JSON/XML.
**Pas de tests automatisés** (à date) — le domain layer est conçu pour en accueillir.

---

## 2. Structure du projet

```
EasySave_Groupe1/
├── src/
│   ├── EasySave.Domain/          # Entités, interfaces, stratégies
│   ├── EasySave.Application/     # Orchestration (JobManager, TransferGate)
│   ├── EasySave.Infrastructure/  # Factories, services, abonnés, parsers
│   ├── EasySave.GUI/             # UI Avalonia + ViewModels
│   ├── EasySave.CLI/             # Point d'entrée ligne de commande
│   └── EasyLog/                  # Bibliothèque de logging autonome
├── Directory.Build.props         # Paramètres MSBuild partagés (TFM, nullable, etc.)
├── EasySave.slnx
├── README.md
└── CONTRIBUTING.md
```

### Dépendances entre projets

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

La couche Domain n'a **aucune dépendance externe** — elle ne connaît pas l'infrastructure, ni l'UI.

---

## 3. Architecture

Le projet suit la **Clean Architecture** : les dépendances vont toujours vers l'intérieur (Domain), jamais vers l'extérieur.

### 3.1 Domain (`EasySave.Domain`)

Contient le cœur métier, sans aucune dépendance sur des frameworks tiers.

**Entités principales :**
- `BackupJob` — un job de sauvegarde nommé avec sa source, sa destination, sa stratégie et sa config de chiffrement
- `BackupConfig` — DTO de création d'un job (passé des couches supérieures vers le domain)
- `FileConfig` — racine désérialisée du `config.json`

**Interfaces clés :**

| Interface | Rôle |
|---|---|
| `IBackupStrategy` | Contrat d'exécution d'une sauvegarde |
| `IPublisher` | Attache / détache des `ISubscriber` |
| `ISubscriber` | Reçoit un `IBackupEvent` lors de la progression |
| `ITransferGate` | Contrôle la priorité et la concurrence des transferts |
| `IEncryptionService` | Abstraction du chiffrement fichier |
| `ISoftwareDetector` | Détecte un processus bloquant |

**Modèles d'événements (records immuables) :**
```
IBackupEvent
  ├── FileTransferReady    (fichier sur le point d'être copié)
  ├── FileTransferSuccess  (copie réussie, inclut durée de transfert et de chiffrement)
  ├── FileTransferFailure  (exception pendant la copie)
  ├── BackupCompleted      (job terminé)
  └── BackupInterrupted    (logiciel bloquant détecté)
```

**Implémentations des stratégies (aussi dans Domain) :**
- `AbstractBackupStrategy` — Template Method + IPublisher ; gère la boucle de copie, la pause, le chiffrement et la publication d'événements
- `FullBackupStrategy` — copie tous les fichiers
- `DifferentialBackupStrategy` — copie uniquement les fichiers dont le `LastWriteTime` est postérieur à la dernière sauvegarde

### 3.2 Application (`EasySave.Application`)

- `JobManager` — cycle de vie complet des jobs : ajout, exécution concurrente (`ConcurrentDictionary<string, Lazy<Task>>`), pause/reprise par `ManualResetEvent`
- `TransferGate` — implémente `ITransferGate` ; deux mécanismes de synchronisation (voir §7)

### 3.3 Infrastructure (`EasySave.Infrastructure`)

| Sous-dossier | Contenu |
|---|---|
| `Factories/` | `AbstractBackupFactory`, `FullBackupFactory`, `DifferentialBackupFactory` — instancient les jobs et câblent les abonnés |
| `Services/` | `CryptoSoftService` (chiffrement via processus externe), `SoftwareDetector` (détection de processus) |
| `Subscribers/` | `StateTracker` (état temps réel → `Logs/states.json`), `DailyLogger` (délègue à EasyLog) |
| `Parsers/` | `JsonParser` — désérialise `FileConfig` depuis un fichier JSON |

### 3.4 GUI (`EasySave.GUI`)

UI Avalonia en MVVM strict avec `CommunityToolkit.Mvvm`.

**ViewModels principaux :**

| ViewModel | Rôle |
|---|---|
| `MainWindowViewModel` | Racine — coordonne tous les onglets |
| `JobsPageViewModel` | Collection de jobs, déclenche start / pause / resume / remove |
| `BackupJobViewModel` | Représente un job dans l'UI ; **implémente `ISubscriber`** pour recevoir les événements et mettre à jour la progression |
| `SettingsPageViewModel` | Extensions prioritaires, apps bloquantes, seuil grands fichiers, format de log |
| `BackupConfigDialogViewModel` | Formulaire de création d'un nouveau job |
| `LogsPageViewModel` | Liste et ouvre les fichiers de log |

**ViewLocator** (`ViewLocator.cs`) — convention automatique : supprime le suffixe `ViewModel` pour résoudre la `View` correspondante par réflexion.

**Liaison UI → thread background :** toutes les mises à jour de `BackupJobViewModel` venant d'un thread worker passent par `Dispatcher.UIThread.InvokeAsync()`.

### 3.5 CLI (`EasySave.CLI`)

Point d'entrée console. Parse les arguments (ex. `1-3`, `1;3`), charge le `config.json`, instancie les jobs via `JobManager`, exécute en `Task.WhenAll()`.

---

## 4. Patterns de conception

| Pattern | Où | Pourquoi |
|---|---|---|
| **Strategy** | `IBackupStrategy` + Full / Differential | Changer l'algorithme de sauvegarde sans modifier `BackupJob` |
| **Observer** | `IPublisher` / `ISubscriber` + événements | Découpler l'exécution de la sauvegarde du logging, du tracking d'état et de l'UI |
| **Abstract Factory** | `AbstractBackupFactory` + sous-classes | Créer un job complet (stratégie + abonnés câblés) en un seul appel |
| **Template Method** | `AbstractBackupStrategy.Execute()` | Définir le squelette de la boucle de copie ; `GetBackupFiles()` est surchargé par les sous-classes |
| **Singleton** | `EasyLog`, `LocalizationService` | Instance unique partagée, thread-safe |
| **DTO** | `BackupConfig`, `FileConfig` | Transport de données entre couches sans exposer les entités domain |
| **View Locator** | `ViewLocator.cs` | Convention-over-configuration pour le mapping VM → View en Avalonia |

---

## 5. Flux d'exécution clés

### Création d'un job (GUI)

```
BackupConfigDialog (UI)
  └─► BackupConfigDialogViewModel.Confirm()
        └─► MainWindowViewModel.AddBackupConfig(BackupConfig)
              └─► JobsPageViewModel : crée BackupJobViewModel (ISubscriber)
              └─► JobManager.AddJob(config)
                    └─► Factory (Full ou Differential)
                          ├─ crée AbstractBackupStrategy
                          ├─ attache StateTracker
                          ├─ attache DailyLogger
                          ├─ attache BackupJobViewModel
                          └─ retourne BackupJob
```

### Exécution d'un job

```
JobsPageViewModel.RunOrPauseJob()
  └─► JobManager.ExecuteJob(name)          ← retourne Task (non bloquant)
        └─► Task.Run(() =>
              BackupJob.Execute(pauseEvent, transferGate)
                └─► AbstractBackupStrategy.Execute()
                      ├─ vérifie ISoftwareDetector → publie BackupInterrupted si bloqué
                      ├─ GetBackupFiles() → IEnumerable<FileInfo> (lazy)
                      ├─ Pour chaque fichier :
                      │    ├─ pauseEvent.WaitOne()          ← point de pause
                      │    ├─ publie FileTransferReady
                      │    ├─ TransferGate.Acquire()        ← priorité / grands fichiers
                      │    ├─ File.Copy()
                      │    ├─ CryptoSoftService.Encrypt()   ← si extension concernée
                      │    ├─ TransferGate.Release()
                      │    └─ publie FileTransferSuccess / Failure
                      └─ publie BackupCompleted
            )
```

### Chiffrement via CryptoSoft

`CryptoSoftService` lance le processus `CryptoSoft` (ou `CryptoSoft.exe` sur Windows) avec le chemin du fichier et la clé en arguments. Le code de retour du processus est interprété comme la durée de chiffrement en millisecondes. Si l'exécutable est introuvable, une `FileNotFoundException` est propagée jusqu'à l'UI sous forme de dialog d'erreur.

---

## 6. Système de configuration

### Fichier `config.json`

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

**Champ `type`** : `"Full"` ou `"Differential"` (insensible à la casse).
**Chiffrement** : `typesToEncrypt` et `encryptKey` sont optionnels ; si absents, aucun chiffrement n'est appliqué.
**`logEmplacement`** : `"local"`, `"server"` ou `"both"` — contrôle où les logs sont écrits.

### Flux de chargement

- **GUI** : le chemin est sélectionné via un dialog au démarrage ; `JsonParser.Parse()` désérialise vers `FileConfig`, puis `JobManager.AddJob()` est appelé pour chaque entrée.
- **CLI** : le chemin est passé en dur ou en premier argument ; même pipeline `JsonParser → FileConfig → JobManager`.

### `AppSettings` (GUI uniquement)

`AppSettings` est un `ObservableObject` qui maintient en mémoire les réglages modifiables depuis l'onglet Paramètres. Les changements sont persistés dans `config.json` à la fermeture ou à la demande.

---

## 7. Synchronisation et concurrence

Deux mécanismes distincts coexistent dans `TransferGate` :

### Priorité d'extension

Les fichiers dont l'extension figure dans `priorityExtensions` sont traités **avant** tous les autres.

- La stratégie trie les fichiers et compte les fichiers prioritaires.
- Les fichiers non-prioritaires appellent `_priorityGate.Wait()` (`ManualResetEventSlim`).
- Quand le dernier fichier prioritaire est traité, `_priorityGate.Set()` débloque les autres.

### Limitation des grands fichiers

Un seul fichier dépassant `largeFileSizeThresholdKb` peut être transféré à la fois.

- `_largeFileLock` est un `SemaphoreSlim(1, 1)`.
- Tout fichier volumineux appelle `_largeFileLock.Wait()` avant la copie et `_largeFileLock.Release()` après.

### Pause / reprise

Chaque job possède un `ManualResetEvent` dédié (initialement signalé = `true`).
- `JobManager.PauseJob(name)` → `pauseEvent.Reset()` — bloque au prochain `WaitOne()` dans la boucle de copie.
- `JobManager.ResumeJob(name)` → `pauseEvent.Set()` — reprend.

---

## 8. Journalisation — EasyLog

`EasyLog` est une bibliothèque autonome (pas de dépendance vers les autres projets EasySave).

- **Singleton** `EasyLog` — point d'entrée unique.
- **Strategy** interne : `JsonLogger` (JSONL — une entrée JSON par ligne) ou `XmlLogger`.
- Le format est choisi par `logFileType` dans `config.json`.
- Les fichiers sont nommés par date et écrits dans `Logs/`.
- `StateTracker` écrit `Logs/states.json` en temps réel avec une écriture atomique (write → `.tmp` → rename).

---

## 9. Conventions de code

Voir `CONTRIBUTING.md` pour le détail complet. Résumé des points clés :

### Nommage C#

| Cible | Convention | Exemple |
|---|---|---|
| Classes, méthodes, propriétés publiques | PascalCase | `JobManager`, `Execute()` |
| Variables locales, paramètres | camelCase | `jobName`, `pauseEvent` |
| Champs privés | _camelCase | `_subscribers`, `_jobManager` |
| Interfaces | IPascalCase | `IBackupStrategy` |
| Enums | PascalCase (singulier) | `BackupType` |
| Events / records d'événements | `[Action][Résultat]` | `FileTransferSuccess` |

### Particularités Avalonia / MVVM

- Les propriétés observables utilisent `[ObservableProperty]` (CommunityToolkit) — le backing field est généré automatiquement.
- Les commandes UI utilisent `[RelayCommand]`.
- Les ViewModels qui ont besoin d'une validation héritent d'`ObservableValidator`.
- Les bindings sont **compilés** (`AvaloniaUseCompiledBindingsByDefault=true`) — les erreurs de binding sont détectées à la compilation.

### Style général

- Records pour les données immuables (événements, métadonnées).
- `init` accessors pour les propriétés settables uniquement à la construction.
- Pattern matching préféré aux casts explicites.
- `Directory.EnumerateFiles` (lazy) plutôt que `GetFiles` (eager).
- Comparaison d'extensions insensible à la casse : `StringComparer.OrdinalIgnoreCase`.
- Le code documente le *comment*, les commentaires documentent le *pourquoi*.

---

## 10. Workflow Git

```
main          ← releases stables uniquement
  └── dev     ← branche d'intégration principale
        └── feat/ma-feature   ← toujours brancher depuis dev
        └── fix/mon-bug
        └── refactor/...
```

- **Commits** : [Conventional Commits](https://www.conventionalcommits.org/) — `feat:`, `fix:`, `refactor:`, `chore:`, `docs:`
- **Branches** : `type/short-description` en kebab-case
- **Versioning** : [SemVer](https://semver.org/) — `Major.Minor.Patch`

---

## 11. Étendre le projet

### Nouvelle stratégie de sauvegarde

1. Créer une classe dans `EasySave.Domain/Strategies/` héritant d'`AbstractBackupStrategy`.
2. Surcharger `GetBackupFiles(string sourcePath)` pour retourner les fichiers à copier.
3. Ajouter une valeur à l'enum `BackupType`.
4. Créer une factory dans `EasySave.Infrastructure/Factories/` héritant d'`AbstractBackupFactory`.
5. Câbler la factory dans `JobManager.AddJob()`.

### Nouvel abonné (logger, notificateur, etc.)

1. Implémenter `ISubscriber` avec `Update(IBackupEvent e)` dans `EasySave.Infrastructure/Subscribers/`.
2. L'enregistrer dans la factory concernée via `Attach(subscriber)`.

### Nouveau format de log

1. Créer une classe dans `EasyLog/` héritant d'`AbstractLogger`.
2. Implémenter `Write(string entry)`.
3. Câbler dans `EasyLog` selon la valeur de `logFileType`.

### Nouvel écran (GUI)

1. Créer un ViewModel dans `EasySave.GUI/ViewModels/` héritant d'`ObservableObject` (ou `ObservableValidator` si validation).
2. Créer la View `.axaml` correspondante dans `EasySave.GUI/Views/` — le `ViewLocator` la résout automatiquement.
3. Exposer le ViewModel depuis `MainWindowViewModel`.

---

## 12. Build & lancement

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- `CryptoSoft` (ou `CryptoSoft.exe` sur Windows) accessible dans le `PATH` pour les fonctionnalités de chiffrement

### Build

```bash
dotnet build EasySave.slnx
```

### Lancer la GUI

```bash
dotnet run --project src/EasySave.GUI
```

### Lancer en CLI

```bash
# Job unique
dotnet run --project src/EasySave.CLI 1

# Plage de jobs (1 à 3)
dotnet run --project src/EasySave.CLI 1-3

# Jobs non-consécutifs
dotnet run --project src/EasySave.CLI 1;3
```

Les numéros correspondent à l'ordre de déclaration des jobs dans `config.json`.

### Publier (binaire autonome)

```bash
dotnet publish src/EasySave.GUI -c Release -r win-x64
dotnet publish src/EasySave.CLI -c Release -r linux-x64
```

`Directory.Build.props` configure la publication en **single-file self-contained** par défaut.
