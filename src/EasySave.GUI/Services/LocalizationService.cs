using System.Globalization;
using System.Resources;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace EasySave.GUI.Services;

public partial class LocalizationService : ObservableObject
{
    public static readonly LocalizationService Instance = new();

    private static readonly ResourceManager ResourceManager = new(
        "EasySave.GUI.Resources.Strings",
        typeof(LocalizationService).Assembly);

    [ObservableProperty] private string _language;

    private LocalizationService()
    {
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
        _language = culture == "fr" ? "fr" : "en";
    }

    partial void OnLanguageChanged(string value)
    {
        OnPropertyChanged(string.Empty); // rafraîchit toutes les propriétés
        OnPropertyChanged("Item[]");     // rafraîchit les bindings indexeur
    }

    [RelayCommand]
    public void ToggleLanguage() => Language = Language == "fr" ? "en" : "fr";

    // Dynamique : affiche la langue vers laquelle basculer
    public string LanguageButton => Language == "fr" ? "EN" : "FR";

    // Indexeur utilisé dans les bindings XAML : {Binding Localization[KeyName]}
    public string this[string key] =>
        ResourceManager.GetString(key, new CultureInfo(Language)) ?? key;
}