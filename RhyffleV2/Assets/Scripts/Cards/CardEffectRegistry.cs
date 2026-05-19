using System;
using System.Collections.Generic;

// NOTE: Dictionary enumeration order — Dictionary<string, Func<ICardEffect>> does not
// guarantee a stable enumeration order by spec. In practice (.NET 5+ / Unity's Mono),
// insertion order is preserved for non-resizing dictionaries, which is sufficient for
// Sprint 1.5 (3 dummy entries). If stricter left→right ordering is needed in Sprint 2,
// switch backing store to List<(string id, Func<ICardEffect> factory)>.
public static class CardEffectRegistry {
    private static readonly Dictionary<string, Func<ICardEffect>> _factories
        = new Dictionary<string, Func<ICardEffect>>();

    public static void Register(string id, Func<ICardEffect> factory) {
        if (string.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
        if (factory == null)          throw new ArgumentNullException(nameof(factory));
        _factories[id] = factory;   // last-write-wins; allows re-registration in tests
    }

    public static bool TryCreate(string id, out ICardEffect effect) {
        if (_factories.TryGetValue(id, out var factory)) {
            effect = factory();
            return effect != null;
        }
        effect = null;
        return false;
    }

    public static IEnumerable<KeyValuePair<string, Func<ICardEffect>>> GetAllFactories() => _factories;

    // For tests — clear registrations between fixtures.
    public static void ClearForTests() => _factories.Clear();
}
