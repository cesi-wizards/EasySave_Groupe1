using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.GUI.Services;

namespace EasySave.GUI.ViewModels;

public abstract class ViewModelBase : ObservableValidator
{
    public LocalizationService Localization => LocalizationService.Instance;
}
