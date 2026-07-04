using LibreHardwareMonitor.Hardware;

namespace RigFanControl.Core;

public record FanCandidate(string Name, Identifier Id);

public static class HardwareDiscovery
{
    public static List<FanCandidate> ListCandidates()
    {
        Computer computer = new() { IsMotherboardEnabled = true };
        computer.Open();

        if (computer.Hardware.Count == 0)
            return [];
        if (computer.Hardware[0].SubHardware.Length == 0)
            return [];
        if (computer.Hardware[0].SubHardware[0].Sensors.Length == 0)
            return [];

        computer.Hardware[0].Update();
        computer.Hardware[0].SubHardware[0].Update();

        List<FanCandidate> list = [..
            computer.Hardware[0].SubHardware[0].Sensors
                .Where(s => s.SensorType == SensorType.Control)
                .Select(s => new FanCandidate(s.Name, s.Identifier))
        ];

        computer.Close();
        return list;
    }
}
