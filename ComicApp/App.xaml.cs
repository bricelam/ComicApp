using System;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using Windows.Storage;

namespace ComicApp;

public partial class App : Application, IServiceProvider
{
    readonly IHost _host;
    Window? _window;

    public App()
    {
        InitializeComponent();

        var builder = Host.CreateApplicationBuilder(
             new HostApplicationBuilderSettings
             {
                 ContentRootPath = AppContext.BaseDirectory
             });

        // TODO: Services

        _host = builder.Build();
    }

    public static new App Current
        => (App)Application.Current;

    public object? GetService(Type serviceType)
        => _host.Services.GetService(serviceType);

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _host.RunAsync();

        var window = new MainWindow();

        var activatedEventArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
        if (activatedEventArgs.Kind == ExtendedActivationKind.File)
        {
            var fileArgs = (Windows.ApplicationModel.Activation.IFileActivatedEventArgs)activatedEventArgs.Data;
            var storageFile = (StorageFile)fileArgs.Files[0];
            window.Open(storageFile);
        }

        window.Activate();

        _window = window;
    }
}
