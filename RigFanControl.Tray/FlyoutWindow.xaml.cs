using RigFanControl.Core;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace RigFanControl.Tray;

public partial class FlyoutWindow : Window
{
    private static readonly Regex _regex = NumberRegex(); //regex that matches disallowed text
    private readonly FanController _controller;

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
        _controller.SetFanSpeed((float)e.NewValue);
    }

    private static bool IsTextAllowed(string text)
    {
        return !_regex.IsMatch(text);
    }

    [GeneratedRegex("[^0-9.-]+")]
    private static partial Regex NumberRegex();
}
