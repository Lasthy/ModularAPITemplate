using System.Reflection;

namespace ModularAPITemplate.SharedKernel.Infrastructure.Events;

/// <summary>
/// Registry used to resolve event CLR types by name when deserializing outbox messages.
/// </summary>
public static class EventTypeRegistry
{
    private static readonly Dictionary<string, Type> _types = new();

    /// <summary>
    /// Registers an event type by its full name.
    /// </summary>
    public static void Register<T>() where T : IEvent
    {
        var type = typeof(T);
        _types[type.FullName!] = type;
    }

    /// <summary>
    /// Resolves a registered event type by its full name.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the event type is not registered.</exception>
    public static Type Resolve(string name)
    {
        if (!_types.TryGetValue(name, out var type))
            throw new InvalidOperationException($"Unknown event type: {name}");

        return type;
    }

    /// <summary>
    /// Registers all concrete event types found in the specified assembly.
    /// </summary>
    public static void RegisterFromAssembly(Assembly assembly)
    {
        var types = assembly
            .GetTypes()
            .Where(t =>
                typeof(IEvent).IsAssignableFrom(t) &&
                !t.IsAbstract &&
                !t.IsInterface);

        foreach (var type in types)
            _types[type.FullName!] = type;
    }
}