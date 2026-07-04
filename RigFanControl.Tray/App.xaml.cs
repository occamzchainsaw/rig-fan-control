using H.NotifyIcon;
using Microsoft.Win32;
using RigFanControl.Core;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RigFanControl.Tray;

public partial class App : Application
{
    private TaskbarIcon? _tray;
    private FlyoutWindow? _flyout;
    private FanController? _controller;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        AppDomain.CurrentDomain.ProcessExit += (_, _) => Cleanup();
        SystemEvents.SessionEnding += (_, _) => Cleanup();

        // todo: change to actual config
        FanControllerConfig config = new()
        {
            ControlIdentifier = "/lpc/nct6687d/0/control/4",
            TachIdentifier = "/lpc/nct6687d/0/fan/4",
            LastValue = 50.0f
        };
        _controller = new FanController(config);
        _flyout = new FlyoutWindow(_controller);

        //_tray = (TaskbarIcon)FindResource("TrayIcon");
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

        var exitItem = new MenuItem { Header = "Exit" };
        exitItem.Click += (_, _) => { Cleanup(); Shutdown(); };

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
        _controller?.Dispose();
        _tray?.Dispose();
    }
}

