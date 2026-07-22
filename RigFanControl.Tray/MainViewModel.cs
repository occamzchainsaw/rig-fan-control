using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Options;
using RigFanControl.Core;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace RigFanControl.Tray;

public partial class MainViewModel(
    IOptionsMonitor<FanControllerConfig> _options,
    IConfigStore _store,
    FanController _controller) : ObservableValidator
{
    private readonly DebounceDispatcher _debounceTimer = new();
    private readonly IntervalDispatcher _intervalTimer = new();

    private bool _initialized = false;

    [ObservableProperty]
    public partial bool IsActive { get; set; }
    [ObservableProperty]
    public partial bool IncompleteConfig { get; set; }
    [ObservableProperty]
    public partial bool FanOn { get; set; }
    [ObservableProperty]
    public partial string FanName { get; set; } = "";
    [ObservableProperty]
    public partial int FanSpeedReading { get; set; }
    [ObservableProperty]
    [Range(0, 100)]
    public partial int SpeedSetting { get; set; }

    partial void OnSpeedSettingChanged(int value) =>
        _debounceTimer.Debounce(500, () => 
        {
            if (!FanOn) return;
            _controller.SetFanSpeed((float)value); 
        });

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
            //if (config.LastValue > 0.0f)
            //    SpeedSetting = (int)Math.Round(config.LastValue);
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

    partial void OnFanOnChanged(bool value)
    {
        if (value)
            _controller.SetFanSpeed((float)SpeedSetting);
        else
            _controller.SetFanSpeed(0.0f);
    }

    private void ReadFanSpeed()
    {
        _controller.UpdateTachReadout();
        var readout = _controller.ReadTachValue() ?? 0.0f;
        FanSpeedReading = (int)Math.Round(readout);
    }

    public void Initialize()
    {
        if (_initialized) return;

        var config = _options.CurrentValue;
        if (string.IsNullOrEmpty(config.ControlIdentifier))
        {
            IncompleteConfig = true;
            return;
        }

        IncompleteConfig = false;
        SpeedSetting = (int)Math.Round(config.LastValue);
        if (!config.StartFanOnStart)
            _controller.SetFanSpeed(0.0f);
        FanOn = config.StartFanOnStart;

        _initialized = true;
    }
}
