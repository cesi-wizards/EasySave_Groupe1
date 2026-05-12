using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using EasySave.GUI.Services;
using EasySave.GUI.ViewModels;
using EasySave.GUI.Views;

namespace EasySave.GUI;

public partial class App : Avalonia.Application
{
    public static LocalizationService Localization { get; } = LocalizationService.Instance;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };
        }

        RegisterGlobalExceptionHandlers();

        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterGlobalExceptionHandlers()
    {
        // Unhandled exceptions on the UI thread (Avalonia dispatcher)
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            e.Handled = true;
            ShowErrorDialog(e.Exception);
        };

        // Unhandled exceptions on background threads
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex)
                Dispatcher.UIThread.Post(() => ShowErrorDialog(ex));
        };

        // Unobserved Task exceptions
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            e.SetObserved();
            Dispatcher.UIThread.Post(() => ShowErrorDialog(e.Exception));
        };
    }

    public static void ShowErrorDialog(Exception ex)
    {
        var owner = (Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;

        var dialog = new ErrorDialog(ex);

        if (owner is not null)
            dialog.ShowDialog(owner);
        else
            dialog.Show();
    }
}
