using LibreHardwareMonitor.Hardware;

namespace RigFanControl.Core;

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

    public FanController(FanControllerConfig config)
    {
        _computer = new() { IsMotherboardEnabled = true };
        _computer.Open();
        SetFromConfig(config);
    }

    public void Dispose()
    {
        _control?.SetDefault();
        _computer.Close();
        GC.SuppressFinalize(this);
    }

    private void SetFromConfig(FanControllerConfig config)
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

    public void UpdateTachReadout() => _tach?.Hardware.Update();

    public float? ReadTachValue() => _tach?.Value;

    public void ReleaseToBios() => _control?.SetDefault();
}
