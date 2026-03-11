using System.Reflection;
using MediatR;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

public static class EventTypeRegistry
{
    private static readonly Dictionary<string, Type> _types = new();

    public static void Register<T>() where T : INotification
    {
        var type = typeof(T);
        _types[type.Name] = type;
    }

    public static Type Resolve(string name)
    {
        if (!_types.TryGetValue(name, out var type))
            throw new InvalidOperationException($"Unknown event type: {name}");

        return type;
    }

    public static void RegisterFromAssembly(Assembly assembly)
    {
        var types = assembly
            .GetTypes()
            .Where(t =>
                typeof(INotification).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface);

        foreach (var type in types)
            _types[type.Name] = type;
    }
}