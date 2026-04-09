using System.Text.Json;
using Cysharp.Serialization.Json;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Json;

public interface IJsonSerializer
{
    string Serialize<T>(T obj);
    T? Deserialize<T>(string json);
    object? Deserialize(string json, Type type);
}

public class JsonSerializer : IJsonSerializer
{
    public string Serialize<T>(T obj)
    {
        return System.Text.Json.JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            Converters =
            {
                new UlidJsonConverter()
            }
        });
    }

    public T? Deserialize<T>(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(json);
    }

    public object? Deserialize(string json, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(json, type);
    }
}