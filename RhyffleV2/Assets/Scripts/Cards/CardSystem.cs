using System.Collections.Generic;
using UnityEngine;

public class CardSystem : MonoBehaviour {
    private readonly List<CardData>    _field         = new List<CardData>();     // max 7
    private readonly List<ICardEffect> _activeEffects = new List<ICardEffect>(); // parallel, left→right activation order

    void Awake() {
        // Pull all currently-registered effect factories from CardEffectRegistry,
        // up to CARDS_IN_FIELD slots. Field is left empty (no CardData) until
        // Task 6 supplies real cards. activeEffects is what actually receives events.
        foreach (var kv in CardEffectRegistry.GetAllFactories()) {
            if (_activeEffects.Count >= GameConfig.CARDS_IN_FIELD) break;
            var effect = kv.Value();
            if (effect == null) continue;
            _activeEffects.Add(effect);
            effect.OnAttach(this);
        }
    }

    void OnDestroy() {
        foreach (var effect in _activeEffects) effect.OnDetach(this);
        _activeEffects.Clear();
        _field.Clear();
    }

    // Read-only snapshot copy for FieldChangedEvent publishers / inspectors.
    public List<CardData> GetField() => new List<CardData>(_field);

    // Aggregate modifiers from all active effects, left→right.
    // GameLoop calls this when applying a NoteJudgedEvent's final score.
    public List<ScoreModifier> GetScoreMultipliers() {
        var result = new List<ScoreModifier>();
        foreach (var effect in _activeEffects) {
            foreach (var m in effect.GetCurrentModifiers()) result.Add(m);
        }
        return result;
    }

    // Sprint 1.5.1: Returns modifiers from the effect at the given slot index only.
    // GameLoop uses this with `note.Line / LANES_PER_CARD` to apply modifiers per area.
    // Returns empty list if cardIndex out of range (e.g., no effect registered at that slot).
    public List<ScoreModifier> GetScoreMultipliersForSlot(int cardIndex) {
        if (cardIndex < 0 || cardIndex >= _activeEffects.Count) {
            return new List<ScoreModifier>();
        }
        var result = new List<ScoreModifier>();
        foreach (var m in _activeEffects[cardIndex].GetCurrentModifiers()) {
            result.Add(m);
        }
        return result;
    }

    // GameLoop calls this on Miss before zeroing combo. Returns true if some
    // active effect protected the combo (effect spends its own charge internally).
    public bool TryConsumeComboProtection() {
        foreach (var effect in _activeEffects) {
            if (effect.TryConsumeComboProtection()) return true;
        }
        return false;
    }

    // For Task 6/8 to populate the field with CardData (convention: one CardData per active effect, by order — NOT validated here, caller's responsibility).
    public void SetField(List<CardData> cards) {
        _field.Clear();
        if (cards != null) _field.AddRange(cards);
    }
}
