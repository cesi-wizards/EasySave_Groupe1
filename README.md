# EasySave

EasySave est un outil d'automatisation de sauvegardes écrit en C# (.NET 10). Il permet de définir, configurer et exécuter des jobs de sauvegarde avec des stratégies paramétrables, un suivi de progression en temps réel et une journalisation structurée.

---

## Fonctionnalités

- **Stratégies complète et différentielle** — copiez tous les fichiers ou uniquement ceux modifiés depuis la dernière sauvegarde.
- **Jobs multiples** — définissez plusieurs jobs nommés, chacun avec sa propre source, destination et stratégie.
- **Notifications par observateurs** — les abonnés (loggers, state tracker) sont notifiés automatiquement de la progression via le pattern Observer.
- **Journalisation structurée** — les événements sont enregistrés en JSON via la DLL `EasyLog`, avec support des logs journaliers et des implémentations personnalisées.
- **Suivi d'état** — l'état courant de chaque job est sérialisé et persisté en fichier pendant l'exécution.
- **Pattern Factory** — `FullBackupFactory` et `DifferentialBackupFactory` gèrent l'instanciation des jobs et le câblage des abonnés de manière transparente.
- **Parsing de configuration JSON** — les jobs sont configurés via des fichiers JSON parsés au démarrage.
- **Interface CLI** — lancez et gérez vos sauvegardes depuis la ligne de commande.

---

## Architecture

Le projet est découpé en quatre assemblies en suivant la **clean architecture** et les principes SOLID :

| Projet | Rôle |
|---|---|
| `EasySave.Domain` | Entités métier (`BackupJob`, `Context`), interfaces (`IBackupStrategy`, `IPublisher`, `ISubscriber`) et implémentations des stratégies |
| `EasySave.Application` | `JobManager`, `MainViewModel` — orchestre le cycle de vie des jobs |
| `EasySave.Infrastructure` | Factories, parsers (`JsonParser`) et abonnés (`DailyLogger`, `StateTracker`) |
| `EasyLog` | Bibliothèque de logging autonome avec l'interface `ILogger` et l'implémentation `JsonLogger` |

Patterns utilisés : **Strategy**, **Observer**, **Abstract Factory**, **Template Method**.

---

## Démarrage

### Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

### Build

```bash
dotnet build EasySave.slnx
```

### Lancement

```bash
dotnet run --project src/EasySave.CLI 1-1
```

---

## Configuration

Les jobs sont définis dans un fichier JSON. Chaque job requiert :

```json
{
  "language": "en",
  "jobs": [
    {
      "name": "MaSauvegarde",
      "sourcePath": "C:/Users/moi/Documents",
      "targetPath": "D:/Sauvegardes/Documents",
      "type": "full"
    },
    {
      "name": "DiffQuotidien",
      "source": "C:/Projets",
      "target": "D:/Sauvegardes/Projets",
      "type": "differential"
    }
  ]
}
```

Le chemin vers ce fichier est passé au démarrage. Le `MainViewModel` le parse et enregistre les jobs dans le `JobManager`.

---

## Utilisation

Pour utiliser l'application, en CLI, il faut lancer l'executable avec des arguments :

- Executer un ou des jobs spécifiques :
```bash
dotnet run --project src/EasySave.CLI 1;3` (exécute les job 1 et 3)
```

- Executer des jobs de façons séquentielle :
```bash
dotnet run --project src/EasySave.CLI 2-4` (exécute les job 2 à 4)
```

Pendant l'exécution, chaque job publie des mises à jour de progression (nombre de fichiers, taille restante, temps de transfert, etc.) à tous les abonnés enregistrés. Le `StateTracker` écrit l'état courant en fichier en temps réel, et le `DailyLogger` ajoute des entrées au log journalier via `EasyLog`.

---

## Étendre le projet

- **Nouvelle stratégie** — implémenter `IBackupStrategy` et ajouter une factory correspondante héritant de `AbstractBackupFactory`.
- **Nouvel abonné** — implémenter `ISubscriber` avec une méthode `Update(Context)` et l'enregistrer globalement via la factory.
- **Nouveau logger** — implémenter `ILogger` avec une méthode `Write(string)` et l'injecter dans `EasyLog`.
