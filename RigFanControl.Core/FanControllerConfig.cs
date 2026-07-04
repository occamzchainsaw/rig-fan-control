namespace RigFanControl.Core;

public class FanControllerConfig
{
    public string ControlIdentifier { get; set; } = "";
    public string TachIdentifier { get; set; } = "";
    public float LastValue = 0.0f;
}
