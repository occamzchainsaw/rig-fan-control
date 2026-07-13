using LibreHardwareMonitor.Hardware;
using Microsoft.Extensions.Options;
using System.Collections;

namespace RigFanControl.Core;

public record FanCandidate(string Name, Identifier Id);

public class FanController : IDisposable
{
    private readonly Computer _computer;
    private IControl? _control = null;
    private ISensor? _tach = null;

    public FanController()
    {
        _computer = new() { IsMotherboardEnabled = true };
        _computer.Open();
    }

    public FanController(IOptionsMonitor<FanControllerConfig> options)
    {
        _computer = new() { IsMotherboardEnabled = true };
        _computer.Open();
        var config = options.CurrentValue;
        SetFromConfig(config);
    }

    public void Dispose()
    {
        _control?.SetDefault();
        _computer.Close();
        GC.SuppressFinalize(this);
    }

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

    public void SetFanSpeed(float value)
    {
        if (_control is null)
            return;

        if (value < _control.MinSoftwareValue || value > _control.MaxSoftwareValue)
            return;

        _control.SetSoftware(value);
    }

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

    public string GetFanName() => _control?.Sensor.Name ?? string.Empty;

    public void UpdateTachReadout() => _tach?.Hardware.Update();

    public float? ReadTachValue() => _tach?.Value;

    public void ReleaseToBios() => _control?.SetDefault();
}
