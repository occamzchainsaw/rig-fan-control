using LibreHardwareMonitor.Hardware;
using RigFanControl.Core;

namespace RigFanCmdTest;

internal class Program
{
    static void Main(string[] args)
    {
        //DumpHardwareReadings();
        Test();
    }

    private static void Test()
    {
        using FanController controller = new();
        List<FanCandidate> candidates = controller.ListCandidates();
        foreach (var candidate in candidates)
            Console.WriteLine($"{candidate.Name} : {candidate.Id}");
        
        var fanCandidate = candidates.First(c => c.Id.ToString().Equals("/lpc/nct6687d/0/control/4", StringComparison.OrdinalIgnoreCase));

        controller.SetFromCandidate(fanCandidate);
        controller.UpdateTachReadout();
        Console.WriteLine($"Current speed: {controller.ReadTachValue()}");
        Console.WriteLine("Setting to 30.0...");
        controller.SetFanSpeed(30.0f);
        Thread.Sleep(5000);
        controller.UpdateTachReadout();
        Console.WriteLine($"Current speed: {controller.ReadTachValue()}");
        Thread.Sleep(5000);
        Console.WriteLine("Setting to 50.0...");
        controller.SetFanSpeed(50.0f);
        Thread.Sleep(5000);
        controller.UpdateTachReadout();
        Console.WriteLine($"Current speed: {controller.ReadTachValue()}");
        Thread.Sleep(5000);
        Console.WriteLine("Setting to 80.0...");
        controller.SetFanSpeed(80.0f);
        Thread.Sleep(5000);
        controller.UpdateTachReadout();
        Console.WriteLine($"Current speed: {controller.ReadTachValue()}");
        Thread.Sleep(5000);
        Console.WriteLine("Setting to max...");
        controller.SetFanSpeed(100.0f);
        Thread.Sleep(5000);
        controller.UpdateTachReadout();
        Console.WriteLine($"Current speed: {controller.ReadTachValue()}");
        controller.ReleaseToBios();
    }

    private static void DumpHardwareReadings()
    {
        Computer computer = new() { IsMotherboardEnabled = true };
        computer.Open();
        foreach (var hw in computer.Hardware)
        {
            hw.Update();
            Console.WriteLine($"{hw.Name} : {hw.HardwareType} : {hw.Identifier}");
            foreach (var sub in hw.SubHardware)
            {
                sub.Update();
                Console.WriteLine($"\t{sub.Name} : {sub.HardwareType} : {sub.Identifier}");
                foreach (var sensor in sub.Sensors)
                {
                    Console.WriteLine($"\t\t{sensor.Name} : {sensor.SensorType} : {sensor.Identifier} : {sensor.Value}");
                    if (sensor.SensorType == SensorType.Control)
                    {
                        var control = sensor.Control;
                        Console.WriteLine($"\t\t\tControl ID: {control.Identifier}");
                    }
                }
            }
        }
    }
}

/*
* MSI B650 GAMING PLUS WIFI (MS-7E26) : Motherboard : /motherboard
*       Nuvoton NCT6687D : SuperIO : /lpc/nct6687d/0
*               CPU Fan : Control : /lpc/nct6687d/0/control/0 : 35
*               Pump Fan : Control : /lpc/nct6687d/0/control/1 : 100
*               System Fan #1 : Control : /lpc/nct6687d/0/control/2 : 60
*               System Fan #2 : Control : /lpc/nct6687d/0/control/3 : 60
*               System Fan #3 : Control : /lpc/nct6687d/0/control/4 : 40    <-
*               System Fan #4 : Control : /lpc/nct6687d/0/control/5 : 60
*               System Fan #5 : Control : /lpc/nct6687d/0/control/6 : 40
*               System Fan #6 : Control : /lpc/nct6687d/0/control/7 : 50
*               +12V : Voltage : /lpc/nct6687d/0/voltage/0 : 12.264001
*               +5V : Voltage : /lpc/nct6687d/0/voltage/1 : 5.09
*               Vcore : Voltage : /lpc/nct6687d/0/voltage/2 : 1.0220001
*               Voltage #1 : Voltage : /lpc/nct6687d/0/voltage/3 : 0.55200005
*               DIMM : Voltage : /lpc/nct6687d/0/voltage/4 : 2.684
*               CPU I/O : Voltage : /lpc/nct6687d/0/voltage/5 : 0.296
*               CPU System Agent : Voltage : /lpc/nct6687d/0/voltage/6 : 0.73200005
*               Voltage #2 : Voltage : /lpc/nct6687d/0/voltage/7 : 1.5480001
*               AVCC3 : Voltage : /lpc/nct6687d/0/voltage/8 : 3.3760002
*               CPU Termination : Voltage : /lpc/nct6687d/0/voltage/9 : 2.046
*               VRef : Voltage : /lpc/nct6687d/0/voltage/10 : 0
*               VSB : Voltage : /lpc/nct6687d/0/voltage/11 : 3.3520002
*               AVSB : Voltage : /lpc/nct6687d/0/voltage/12 : 3.3520002
*               CMOS Battery : Voltage : /lpc/nct6687d/0/voltage/13 : 3.0960002
*               CPU : Temperature : /lpc/nct6687d/0/temperature/0 : 50
*               System : Temperature : /lpc/nct6687d/0/temperature/1 : 39
*               VRM MOS : Temperature : /lpc/nct6687d/0/temperature/2 : 37
*               PCH : Temperature : /lpc/nct6687d/0/temperature/3 : 41
*               CPU Socket : Temperature : /lpc/nct6687d/0/temperature/4 : 34
*               PCIe x1 : Temperature : /lpc/nct6687d/0/temperature/5 : 80.5
*               M2 #1 : Temperature : /lpc/nct6687d/0/temperature/6 : 0
*               CPU Fan : Fan : /lpc/nct6687d/0/fan/0 : 843
*               Pump Fan : Fan : /lpc/nct6687d/0/fan/1 : 0
*               System Fan #1 : Fan : /lpc/nct6687d/0/fan/2 : 878
*               System Fan #3 : Fan : /lpc/nct6687d/0/fan/4 : 1530  <-
*               System Fan #4 : Fan : /lpc/nct6687d/0/fan/5 : 950
*               System Fan #2 : Fan : /lpc/nct6687d/0/fan/3 : 565
*               System Fan #5 : Fan : /lpc/nct6687d/0/fan/6 : 0
*               System Fan #6 : Fan : /lpc/nct6687d/0/fan/7 : 0
*/
