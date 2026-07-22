namespace RigFanControl.Core;

/// <summary>Persisted settings identifying the selected fan and its last applied speed.</summary>
public class FanControllerConfig
{
    /// <summary>Identifier of the control sensor driving the fan.</summary>
    public string ControlIdentifier { get; set; } = "";
    /// <summary>Identifier of the tachometer sensor reporting the fan's RPM.</summary>
    public string TachIdentifier { get; set; } = "";
    /// <summary>Last fan speed percentage applied.</summary>
    public float LastValue { get; set; } = 0.0f;
    public bool StartFanOnStart { get; set; } = false;
}
