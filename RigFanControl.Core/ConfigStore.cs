using System.Text.Encodings.Web;
using System.Text.Json;

namespace RigFanControl.Core;

/// <summary>Persists fan controller configuration.</summary>
public interface IConfigStore
{
    /// <summary>Writes the given configuration to storage.</summary>
    public void Save(FanControllerConfig config);
}

/// <summary>Stores configuration as an indented JSON file at the given path.</summary>
public sealed class JsonConfigStore(string _path) : IConfigStore
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>Serializes the configuration and writes it to the file path.</summary>
    public void Save(FanControllerConfig config)
    {
        var serializedData = JsonSerializer.Serialize(config, _serializerOptions);
        File.WriteAllText(_path, serializedData);
    }
}
