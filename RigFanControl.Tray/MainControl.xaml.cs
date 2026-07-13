using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace RigFanControl.Tray;
/// <summary>
/// Interaction logic for MainControl.xaml
/// </summary>
public partial class MainControl : UserControl
{
    [GeneratedRegex("[^0-9.-]+")]
    private static partial Regex NumberRegex();
    public MainViewModel ViewModel { get; set; }
    public event EventHandler? SettingsRequested;

    public MainControl(MainViewModel viewmodel)
    {
        ViewModel = viewmodel;
        InitializeComponent();

        this.DataContext = ViewModel;
    }

    private void SettingsClicked(object sender, System.Windows.RoutedEventArgs e) =>
        SettingsRequested?.Invoke(this, EventArgs.Empty);

    private void PreviewTextInputHandler(object sender, TextCompositionEventArgs e) =>
        e.Handled = !IsTextAllowed(e.Text);

    private static bool IsTextAllowed(string text) => 
        !NumberRegex().IsMatch(text);

    private void GridMouseDown(object sender, MouseButtonEventArgs e) => 
        Keyboard.ClearFocus();

    private void SpeedTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Keyboard.ClearFocus();
    }
}
