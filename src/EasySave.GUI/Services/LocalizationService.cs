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
        OnPropertyChanged(string.Empty); // refresh properties
        OnPropertyChanged("Item[]");     // refresh binding
    }

    [RelayCommand]
    public void ToggleLanguage() => Language = Language == "fr" ? "en" : "fr";

    // Dynamic : display language to change
    public string LanguageButton => Language == "fr" ? "EN" : "FR";

    public string this[string key] =>
        ResourceManager.GetString(key, new CultureInfo(Language)) ?? key;
}
