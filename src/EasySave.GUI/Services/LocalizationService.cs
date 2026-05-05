using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EasySave.GUI.Services;

public partial class LocalizationService : ObservableObject
{
    public static readonly LocalizationService Instance = new();

    [ObservableProperty] private string _language;

    private LocalizationService()
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _language = culture == "fr" ? "fr" : "en";
    }

    private bool IsFr => Language == "fr";

    partial void OnLanguageChanged(string value)
    {
        OnPropertyChanged(string.Empty);
    }

    [RelayCommand]
    public void ToggleLanguage() => Language = IsFr ? "en" : "fr";

    // Button label showing the language to switch TO
    public string LanguageButton => IsFr ? "EN" : "FR";

    // MainWindow
    public string NewBackup => IsFr ? "+ Nouvelle sauvegarde" : "+ New Backup";
    public string SettingsButton => IsFr ? "⚙ Paramètres" : "⚙ Settings";

    // BackupConfigDialog
    public string BackupConfigTitle => IsFr ? "Nouvelle configuration de sauvegarde" : "New Backup Configuration";
    public string NameLabel => IsFr ? "Nom" : "Name";
    public string BackupNamePlaceholder => IsFr ? "Nom de la sauvegarde" : "Backup name";
    public string SelectSourceDirectory => IsFr ? "Sélectionner le répertoire source" : "Select the source directory";
    public string SelectTargetDirectory => IsFr ? "Sélectionner le répertoire cible" : "Select the target directory";
    public string BackupTypeLabel => IsFr ? "Type de sauvegarde" : "Backup Type";
    public string EncryptionKeyLabel => IsFr ? "Clé de chiffrement" : "Encryption Key";
    public string EncryptionKeyPlaceholder => IsFr
        ? "Entrer la clé de chiffrement (laisser vide pour aucun chiffrement)"
        : "Enter encryption key (leave empty for no encryption)";
    public string EncryptedExtensionsLabel => IsFr ? "Extensions chiffrées" : "Encrypted Extensions";
    public string AddButton => IsFr ? "Ajouter" : "Add";
    public string CreateButton => IsFr ? "Créer" : "Create";
    public string SelectFolderTitle => IsFr ? "Sélectionner un dossier" : "Select a Folder";

    // SettingsDialog
    public string SettingsTitle => IsFr ? "Paramètres" : "Settings";
    public string LogFileTypeLabel => IsFr ? "Type de fichier de journal" : "Log File Type";
    public string BlockingApplicationLabel => IsFr ? "Application bloquante" : "Blocking Application";
    public string BlockingApplicationDescription => IsFr
        ? "Si ce processus est en cours d'exécution, aucune tâche de sauvegarde ne peut être démarrée."
        : "If this process is running, no backup job can be started.";
    public string BlockingAppPlaceholder => IsFr ? "ex. outlook, teams, notepad" : "e.g. outlook, teams, notepad";

    // Common
    public string CancelButton => IsFr ? "Annuler" : "Cancel";
    public string SaveButton => IsFr ? "Enregistrer" : "Save";
}
