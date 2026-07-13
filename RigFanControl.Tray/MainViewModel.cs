using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using RigFanControl.Core;
using System.ComponentModel.DataAnnotations;

namespace RigFanControl.Tray;

public partial class MainViewModel(
    IOptionsMonitor<FanControllerConfig> _options,
    IConfigStore _store,
    FanController _controller) : ObservableValidator
{
    private readonly DebounceDispatcher _debounceTimer = new();
    private readonly IntervalDispatcher _intervalTimer = new();

    [ObservableProperty]
    public partial bool IsActive { get; set; }
    [ObservableProperty]
    public partial bool IncompleteConfig { get; set; }
    [ObservableProperty]
    public partial string FanName { get; set; } = "";
    [ObservableProperty]
    public partial int FanSpeedReading { get; set; }
    [ObservableProperty]
    [Range(0, 100)]
    public partial int SpeedSetting { get; set; }

    partial void OnSpeedSettingChanged(int value) =>
        _debounceTimer.Debounce(500, () => { _controller.SetFanSpeed((float)value); });

    partial void OnIsActiveChanged(bool value)
    {
        if (value)
        {
            var config = _options.CurrentValue;
            if (string.IsNullOrEmpty(config.ControlIdentifier))
            {
                IncompleteConfig = true;
                return;
            }

            IncompleteConfig = false;
            if (config.LastValue > 0.0f)
                SpeedSetting = (int)Math.Round(config.LastValue);
            FanName = _controller.GetFanName();
            ReadFanSpeed(); // one immediate read
            _intervalTimer.Start(500, ReadFanSpeed);
        }
        else
        {
            _intervalTimer.Stop();
            if (!IncompleteConfig && SpeedSetting > 0)
            {
                var config = _options.CurrentValue;
                config.LastValue = (float)SpeedSetting;
                _store.Save(config);
            }
        }
    }

    private void ReadFanSpeed()
    {
        _controller.UpdateTachReadout();
        var readout = _controller.ReadTachValue() ?? 0.0f;
        FanSpeedReading = (int)Math.Round(readout);
    }
}
