using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using RigFanControl.Core;
using System.Collections.ObjectModel;

namespace RigFanControl.Tray;

public partial class SettingsViewModel(
    IOptionsMonitor<FanControllerConfig> _options,
    IConfigStore _store,
    FanController _controller) : ObservableObject
{
    public ObservableCollection<FanCandidate> FanCandidates { get; set; } = [];
    [ObservableProperty]
    public partial FanCandidate? SelectedCandidate { get; set; } = null;
    public event Action? OnSaved;

    private void CheckConfig()
    {
        var config = _options.CurrentValue;
        if (string.IsNullOrEmpty(config.ControlIdentifier)) return;

        SelectedCandidate = FanCandidates
            .FirstOrDefault(fc => fc.Id.ToString().Equals(config.ControlIdentifier, StringComparison.OrdinalIgnoreCase));
    }

    public void LoadCandidates()
    {
        FanCandidates = [.. HardwareDiscovery.ListCandidates()];
        CheckConfig();
    }

    [RelayCommand]
    private void Save()
    {
        if (SelectedCandidate == null) return;

        var config = _options.CurrentValue;
        config.ControlIdentifier = SelectedCandidate.Id.ToString();
        var tach = HardwareDiscovery.GetTachFromControl(SelectedCandidate.Id);
        config.TachIdentifier = tach?.ToString() ?? string.Empty;
        _store.Save(config);

        _controller.SetFromConfig(config);

        OnSaved?.Invoke();
    }
}
