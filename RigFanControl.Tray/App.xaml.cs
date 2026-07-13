using H.NotifyIcon;
using Microsoft.Win32;
using RigFanControl.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace RigFanControl.Tray;

public partial class App : Application
{
    private ServiceProvider _provider = null!;
    private TaskbarIcon? _tray;
    private FlyoutWindow? _flyout;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RigFanControl");
        string configPath = Path.Combine(appDataDir, "config.json");

        var store = new JsonConfigStore(configPath);
        if (!Directory.Exists(appDataDir))
            Directory.CreateDirectory(appDataDir);
        if (!File.Exists(configPath))
            store.Save(new FanControllerConfig());

        var configuration = new ConfigurationBuilder()
            .AddJsonFile(configPath, optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection();

        services.AddOptions<FanControllerConfig>()
            .Bind(configuration)
            .Validate(c => c.LastValue is >= 0 and <= 100);

        services.AddSingleton<IConfigStore>(_ => new JsonConfigStore(configPath));
        services.AddSingleton<FanController>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<SettingsControl>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainControl>();
        services.AddSingleton<FlyoutWindow>();

        _provider = services.BuildServiceProvider();

        _flyout = _provider.GetRequiredService<FlyoutWindow>();

        AppDomain.CurrentDomain.ProcessExit += (_, _) => Cleanup();
        SystemEvents.SessionEnding += (_, _) => Cleanup();

        _tray = new()
        {
            IconSource = new BitmapImage(new Uri("pack://application:,,,/Assets/trayicon.ico")),
            ToolTipText = "Rig Fan Control",
            Visibility = Visibility.Visible
        };
        _tray.TrayLeftMouseDown += (_, _) => ToggleFlyout();
        _tray.ContextMenu = BuildContextMenu();
        _tray.ForceCreate(enablesEfficiencyMode: false);
    }

    private ContextMenu BuildContextMenu()
    {
        var menu = new ContextMenu();

        var openItem = new MenuItem { Header = "Open" };
        openItem.Click += (_, _) => ToggleFlyout();
        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => { Cleanup(); Shutdown(); };

        menu.Items.Add(openItem);
        menu.Items.Add(exitItem);
        return menu;
    }

    private void ToggleFlyout()
    {
        if (_flyout!.IsVisible)
            _flyout.Hide();
        else
            _flyout.ShowNearTray();
    }

    private void Cleanup()
    {
        _provider?.Dispose();
        _tray?.Dispose();
    }
}
