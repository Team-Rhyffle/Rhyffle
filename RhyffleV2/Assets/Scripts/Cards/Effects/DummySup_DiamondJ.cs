using System.Collections.Generic;

// 지원형 더미 — 필드에 같은 suit가 3장 이상이면 모든 점수 ×1.20 (지속).
// Pattern: on FieldChangedEvent, recompute _active by counting suits.
// _active stays true until next FieldChangedEvent says otherwise.
public class DummySup_DiamondJ : ICardEffect {
    public const int SUIT_TRIGGER_COUNT = 3;
    public string Id => "DummySup_DiamondJ";
    private bool _active;
    private System.Action<FieldChangedEvent> _handler;

    public void OnAttach(CardSystem ctx) {
        _handler = OnFieldChanged;
        EventBus.Subscribe(_handler);
    }
    public void OnDetach(CardSystem ctx) {
        if (_handler != null) EventBus.Unsubscribe(_handler);
        _handler = null;
        _active = false;
    }
    private void OnFieldChanged(FieldChangedEvent evt) {
        _active = false;
        if (evt.Field == null) return;
        var counts = new Dictionary<CardSuit, int>();
        foreach (var card in evt.Field) {
            if (card == null) continue;
            counts.TryGetValue(card.Suit, out var n);
            counts[card.Suit] = n + 1;
        }
        foreach (var c in counts.Values) {
            if (c >= SUIT_TRIGGER_COUNT) { _active = true; break; }
        }
    }
    public IEnumerable<ScoreModifier> GetCurrentModifiers() {
        if (_active) yield return new ScoreModifier(1.20f, Id);
    }
    public bool TryConsumeComboProtection() => false;
}
