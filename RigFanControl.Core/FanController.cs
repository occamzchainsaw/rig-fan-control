using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Options;
using System.Collections;

namespace RigFanControl.Core;

/// <summary>A selectable fan control sensor with its display name and identifier.</summary>
public record FanCandidate(string Name, Identifier Id);

/// <summary>Discovers motherboard fan sensors and drives fan speed via LibreHardwareMonitor.</summary>
public class FanController : IDisposable
{
    private readonly Computer _computer;
    private IControl? _control = null;
    private ISensor? _tach = null;

    /// <summary>Opens the hardware monitor without selecting a fan.</summary>
    public FanController()
    {
        _computer = new() { IsMotherboardEnabled = true };
        _computer.Open();
    }

    /// <summary>Opens the hardware monitor and selects the fan described by config.</summary>
    public FanController(IOptionsMonitor<FanControllerConfig> options)
    {
        _computer = new() { IsMotherboardEnabled = true };
        _computer.Open();
        var config = options.CurrentValue;
        SetFromConfig(config);
    }

    /// <summary>Returns the fan to BIOS control and closes the hardware monitor.</summary>
    public void Dispose()
    {
        _control?.SetDefault();
        _computer.Close();
        GC.SuppressFinalize(this);
    }

    /// <summary>Selects the control and tach sensors matching the identifiers in config.</summary>
    public void SetFromConfig(FanControllerConfig config)
    {
        if (_computer.Hardware.Count == 0)
            return;
        _computer.Hardware[0].Update();

        if (_computer.Hardware[0].SubHardware.Length == 0)
            return;

        var superIo = _computer.Hardware[0].SubHardware[0];
        if (superIo.Sensors.Length == 0)
            return;

        superIo.Update();

        var comp = StringComparison.OrdinalIgnoreCase;
        _control = superIo.Sensors
            .Where(s => s.SensorType == SensorType.Control)
            .FirstOrDefault(s => s.Identifier.ToString().Equals(config.ControlIdentifier, comp))?.Control;

        _tach = superIo.Sensors
            .Where(s => s.SensorType == SensorType.Fan)
            .FirstOrDefault(s => s.Identifier.ToString().Equals(config.TachIdentifier, comp));
    }

    /// <summary>Selects the given candidate as the active control and finds its matching tach sensor.</summary>
    public void SetFromCandidate(FanCandidate candidate)
    {
        if (_computer.Hardware.Count == 0)
            return;
        _computer.Hardware[0].Update();

        if (_computer.Hardware[0].SubHardware.Length == 0)
            return;

        var superIo = _computer.Hardware[0].SubHardware[0];
        if (superIo.Sensors.Length == 0)
            return;

        superIo.Update();

        _control = superIo.Sensors
            .Where(s => s.SensorType == SensorType.Control)
            .FirstOrDefault(s => s.Identifier == candidate.Id)?.Control;

        string tachId = candidate.Id.ToString().Replace(@"control", @"fan");
        _tach = superIo.Sensors
            .Where(s => s.SensorType == SensorType.Fan)
            .FirstOrDefault(s => s.Identifier.ToString().Equals(tachId, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>Sets the fan to the given speed percentage, ignoring out-of-range values.</summary>
    public void SetFanSpeed(float value)
    {
        if (_control is null)
            return;

        if (value < _control.MinSoftwareValue || value > _control.MaxSoftwareValue)
            return;

        _control.SetSoftware(value);
    }

    /// <summary>Returns all available fan control sensors as selectable candidates.</summary>
    public List<FanCandidate> ListCandidates()
    {
        if (_computer.Hardware.Count == 0)
            return [];
        if (_computer.Hardware[0].SubHardware.Length == 0)
            return [];

        var superIo = _computer.Hardware[0].SubHardware[0];
        if (superIo.Sensors.Length == 0)
            return [];

        superIo.Update();

        return [.. superIo.Sensors
            .Where(s => s.SensorType == SensorType.Control)
            .Select(s => new FanCandidate(s.Name, s.Identifier))
        ];
    }

    /// <summary>Finds the tach sensor identifier paired with the given control sensor, or null.</summary>
    public Identifier? GetTachFromControl(Identifier controlId)
    {
        if (_computer.Hardware.Count == 0)
            return null;
        if (_computer.Hardware[0].SubHardware.Length == 0)
            return null;

        var superIo = _computer.Hardware[0].SubHardware[0];
        if (superIo.Sensors.Length == 0)
            return null;

        superIo.Update();

        var searchString = controlId.ToString().Replace(@"control", @"fan");
        return _computer.Hardware[0].SubHardware[0].Sensors
            .Where(s => s.SensorType == SensorType.Fan)
            .FirstOrDefault(s => s.Identifier.ToString().Equals(searchString, StringComparison.OrdinalIgnoreCase))
            ?.Identifier;
    }

    /// <summary>Returns the selected fan's display name, or empty if none selected.</summary>
    public string GetFanName() => _control?.Sensor.Name ?? string.Empty;

    /// <summary>Refreshes the tach sensor's hardware readings.</summary>
    public void UpdateTachReadout() => _tach?.Hardware.Update();

    /// <summary>Returns the current fan RPM, or null if no tach is selected.</summary>
    public float? ReadTachValue() => _tach?.Value;

    /// <summary>Hands fan control back to the BIOS.</summary>
    public void ReleaseToBios() => _control?.SetDefault();
}
