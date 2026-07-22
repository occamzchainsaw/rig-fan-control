using System.Windows.Controls;

namespace RigFanControl.Tray;
/// <summary>
/// Interaction logic for SettingsControl.xaml
/// </summary>
public partial class SettingsControl : UserControl
{
    public SettingsViewModel ViewModel { get; set; }
    public event EventHandler? HideRequested;

    public SettingsControl(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();

        ViewModel.OnSaved += () => { HideRequested?.Invoke(this, EventArgs.Empty); };

        this.DataContext = ViewModel;
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ViewModel.Initialize();
    }

    private void CancelClicked(object sender, System.Windows.RoutedEventArgs e)
    {
        HideRequested?.Invoke(this, EventArgs.Empty);
    }
}
