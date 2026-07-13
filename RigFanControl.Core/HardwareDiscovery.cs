using LibreHardwareMonitor.Hardware;

namespace RigFanControl.Core;

public record FanCandidate(string Name, Identifier Id);

public static class HardwareDiscovery
{
    private static Computer? GetUpdatedHardware()
    {
        Computer computer = new() { IsMotherboardEnabled = true };
        computer.Open();

        if (computer.Hardware.Count == 0)
            return null;
        if (computer.Hardware[0].SubHardware.Length == 0)
            return null;
        if (computer.Hardware[0].SubHardware[0].Sensors.Length == 0)
            return null;

        computer.Hardware[0].Update();
        computer.Hardware[0].SubHardware[0].Update();
        return computer;
    }

    public static List<FanCandidate> ListCandidates()
    {
        if (GetUpdatedHardware() is not Computer computer)
            return [];

        List<FanCandidate> list = [..
            computer.Hardware[0].SubHardware[0].Sensors
                .Where(s => s.SensorType == SensorType.Control)
                .Select(s => new FanCandidate(s.Name, s.Identifier))
        ];

        computer.Close();
        return list;
    }

    public static Identifier? GetTachFromControl(Identifier controlId)
    {
        if (GetUpdatedHardware() is not Computer computer)
            return null;

        var searchString = controlId.ToString().Replace(@"control", @"fan");

        return computer.Hardware[0].SubHardware[0].Sensors
            .Where(s => s.SensorType == SensorType.Fan)
            .Where(s => s.Identifier.ToString().Equals(searchString, StringComparison.OrdinalIgnoreCase))
            .Select(s => s.Identifier)
            .FirstOrDefault();
    }
}
