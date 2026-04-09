using System.Text.Json;
using Cysharp.Serialization.Json;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Json;

public interface IJsonSerializer
{
    string Serialize(object obj);
    T? Deserialize<T>(string json);
    object? Deserialize(string json, Type type);
}

public class JsonSerializer : IJsonSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    public string Serialize(object obj)
    {
        return System.Text.Json.JsonSerializer.Serialize<object>(obj, SerializerOptions);
    }

    public T? Deserialize<T>(string json)
    {
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, SerializerOptions);
    }

    public object? Deserialize(string json, Type type)
    {
        return System.Text.Json.JsonSerializer.Deserialize(json, type, SerializerOptions);
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new UlidJsonConverter());

        return options;
    }
}