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
    [ObservableProperty]
    public partial bool StartWithWindows { get; set; } = StartupManager.IsEnabled();
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
        var candidates = _controller.ListCandidates();
        foreach (var candidate in candidates)
        {
            FanCandidates.Add(candidate);
        }
        CheckConfig();
    }

    [RelayCommand]
    private void Save()
    {
        if (SelectedCandidate == null) return;

        var config = _options.CurrentValue;
        config.ControlIdentifier = SelectedCandidate.Id.ToString();
        var tach = _controller.GetTachFromControl(SelectedCandidate.Id);
        config.TachIdentifier = tach?.ToString() ?? string.Empty;
        _store.Save(config);

        _controller.SetFromConfig(config);

        if (StartWithWindows)
            StartupManager.Enable();
        else
            StartupManager.Disable();

        OnSaved?.Invoke();
    }
}
