namespace RigFanControl.Core;

public class FanControllerConfig
{
    public string ControlIdentifier { get; set; } = "";
    public string TachIdentifier { get; set; } = "";
    public float LastValue { get; set; } = 0.0f;
    public bool StartFanOnStart { get; set; } = false;
}
