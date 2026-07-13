using RigFanControl.Core;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace RigFanControl.Tray;

public partial class FlyoutWindow : Window
{
    [GeneratedRegex("[^0-9.-]+")]
    private static partial Regex NumberRegex();
    private readonly FanController _controller;
    private readonly DebounceDispatcher _debounceTimer = new();

    public FlyoutWindow(FanController controller)
    {
        _controller = controller;
        InitializeComponent();
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        Hide();
    }

    public void ShowNearTray()
    {
        var area = SystemParameters.WorkArea;
        Left = area.Right - Width - 12;
        Top = area.Bottom - Height - 12;
        Show();
        Activate();
    }

    private void PreviewTextInputHandler(object sender, TextCompositionEventArgs e)
    {
        e.Handled = !IsTextAllowed(e.Text);
    }

    private void SetValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _debounceTimer.Debounce(500, () => { _controller.SetFanSpeed((float)e.NewValue); });
    }

    private static bool IsTextAllowed(string text)
    {
        return !NumberRegex().IsMatch(text);
    }
}
