using System;
using System.Collections.Generic;

/// <summary>
/// Static generic pub/sub event bus. MainThread-only — no locking.
/// Usage:
///   EventBus.Subscribe&lt;MyEvent&gt;(OnMyEvent);
///   EventBus.Publish(new MyEvent { ... });
///   EventBus.Unsubscribe&lt;MyEvent&gt;(OnMyEvent);
/// </summary>
public static class EventBus {
    static readonly Dictionary<Type, List<Delegate>> _handlers =
        new Dictionary<Type, List<Delegate>>();

    /// <summary>Register <paramref name="handler"/> for events of type <typeparamref name="T"/>.</summary>
    /// <exception cref="ArgumentNullException">Thrown if handler is null.</exception>
    public static void Subscribe<T>(Action<T> handler) {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        var key = typeof(T);
        if (!_handlers.TryGetValue(key, out var list)) {
            list = new List<Delegate>();
            _handlers[key] = list;
        }
        list.Add(handler);
    }

    /// <summary>Remove <paramref name="handler"/> from the subscriber list for type <typeparamref name="T"/>.</summary>
    /// <exception cref="ArgumentNullException">Thrown if handler is null.</exception>
    public static void Unsubscribe<T>(Action<T> handler) {
        if (handler == null) throw new ArgumentNullException(nameof(handler));

        var key = typeof(T);
        if (_handlers.TryGetValue(key, out var list)) {
            list.Remove(handler);
        }
    }

    /// <summary>
    /// Invoke all handlers registered for type <typeparamref name="T"/> with <paramref name="evt"/>.
    /// Iterates a snapshot so handlers may safely unsubscribe during dispatch.
    /// No-op if no handlers are registered.
    /// </summary>
    public static void Publish<T>(T evt) {
        var key = typeof(T);
        if (!_handlers.TryGetValue(key, out var list) || list.Count == 0) return;

        // Defensive snapshot — handlers may unsubscribe themselves during dispatch.
        var snapshot = new List<Delegate>(list);
        foreach (var d in snapshot) {
            ((Action<T>)d)(evt);
        }
    }
}
