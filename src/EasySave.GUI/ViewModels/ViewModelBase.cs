using CommunityToolkit.Mvvm.ComponentModel;
using EasySave.GUI.Services;

namespace EasySave.GUI.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    public LocalizationService Localization => LocalizationService.Instance;
}
