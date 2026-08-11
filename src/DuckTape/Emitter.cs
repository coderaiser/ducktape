namespace DuckTape;

public class Emitter
{
    readonly Dictionary<string, List<Action<object?>>> _listeners = new();

    public void On(string @event, Action<object?> fn) =>
        (_listeners.TryGetValue(@event, out var list)
            ? list
            : _listeners[@event] = new()).Add(fn);

    public void Off(string @event, Action<object?> fn) =>
        _listeners.GetValueOrDefault(@event)?.Remove(fn);

    public void Emit(string @event, object? data = null)
    {
        foreach (var fn in _listeners.GetValueOrDefault(@event) ?? [])
            fn(data);
    }
}
