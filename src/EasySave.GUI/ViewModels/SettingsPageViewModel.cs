using EasySave.Application;

namespace EasySave.GUI.ViewModels;

public partial class SettingsPageViewModel : ViewModelBase
{
    private readonly JobManager _jobManager;
    private readonly AppSettings _settings;

    public SettingsPageViewModel(JobManager jobManager, AppSettings settings)
    {
        _jobManager = jobManager;
        _settings = settings;

        settings.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(AppSettings.LogFileType):
                    OnPropertyChanged(nameof(IsJsonLog));
                    OnPropertyChanged(nameof(IsXmlLog));
                    break;
                case nameof(AppSettings.LogEmplacement):
                    OnPropertyChanged(nameof(IsLocalLog));
                    OnPropertyChanged(nameof(IsServerLog));
                    OnPropertyChanged(nameof(IsServerAndLocalLog));
                    break;
                case nameof(AppSettings.BlockingApp):
                    OnPropertyChanged(nameof(BlockingApp));
                    var softwares = settings.BlockingApp.Split(',',
                        System.StringSplitOptions.RemoveEmptyEntries | System.StringSplitOptions.TrimEntries);
                    _jobManager.SetBusinessSoftwares(softwares);
                    break;
            }
        };
    }

    public string BlockingApp
    {
        get => _settings.BlockingApp;
        set => _settings.BlockingApp = value;
    }

    public bool IsJsonLog
    {
        get => _settings.LogFileType == "JSON";
        set { if (value) _settings.LogFileType = "JSON"; }
    }

    public bool IsXmlLog
    {
        get => _settings.LogFileType == "XML";
        set { if (value) _settings.LogFileType = "XML"; }
    }

    public bool IsLocalLog
    {
        get => _settings.LogEmplacement == "local";
        set { if (value) _settings.LogEmplacement = "local"; }
    }

    public bool IsServerLog
    {
        get => _settings.LogEmplacement == "server";
        set { if (value) _settings.LogEmplacement = "server"; }
    }

    public bool IsServerAndLocalLog
    {
        get => _settings.LogEmplacement == "both";
        set { if (value) _settings.LogEmplacement = "both"; }
    }
}
