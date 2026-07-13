using System.Text.Encodings.Web;
using System.Text.Json;

namespace RigFanControl.Core;

public interface IConfigStore
{
    public void SaveConfig(FanControllerConfig config);
}

public sealed class JsonConfigStore(string _path) : IConfigStore
{
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    public void SaveConfig(FanControllerConfig config)
    {
        var serializedData = JsonSerializer.Serialize(config, _serializerOptions);
        File.WriteAllText(_path, serializedData);
    }
}
