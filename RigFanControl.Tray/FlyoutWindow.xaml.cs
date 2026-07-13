using RigFanControl.Core;
using System.Windows;
using System.Windows.Controls;

namespace RigFanControl.Tray;

public partial class FlyoutWindow : Window
{
    private readonly MainControl _mainControl;
    private readonly SettingsControl _settingsControl;

    public FlyoutWindow(
        FanController controller,
        MainControl mainControl,
        SettingsControl settingsControl)
    {
        InitializeComponent();
        this.SizeChanged += SetPosition;
        
        _settingsControl = settingsControl;
        _mainControl = mainControl;

        _mainControl.SettingsRequested += (s, e) => SwitchTo(_settingsControl);
        _settingsControl.HideRequested += (s, e) => SwitchTo(_mainControl);

        MainContainer.Children.Add(_mainControl);
        MainContainer.Children.Add(_settingsControl);

        SwitchTo(_mainControl);
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        if (_mainControl.DataContext is MainViewModel vm)
            vm.IsActive = false;
        Hide();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        if (_mainControl.DataContext is MainViewModel vm)
            vm.IsActive = true;
    }

    public void ShowNearTray()
    {
        Show();
        Activate();
    }

    private void SetPosition(object sender, SizeChangedEventArgs e)
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right - ActualWidth - 12;
        Top = area.Bottom - ActualHeight - 12;
    }

    private void SwitchTo(UserControl activeControl)
    {
        _mainControl.Visibility = Visibility.Collapsed;
        _settingsControl.Visibility = Visibility.Collapsed;

        if (_mainControl.DataContext is MainViewModel vm)
            vm.IsActive = activeControl == _mainControl;

        activeControl.Visibility = Visibility.Visible;
    }
}
