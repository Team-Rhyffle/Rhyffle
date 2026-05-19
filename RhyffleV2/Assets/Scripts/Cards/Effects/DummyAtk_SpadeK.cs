using System.Collections.Generic;

// 공격형 더미 — Perfect/Great 시 다음 점수 계산에 ×1.10 modifier 적용.
// State: _activeThisJudgment — set true on Perfect/Great NoteJudgedEvent,
// false otherwise (so the modifier only applies to the *current* note, not subsequent ones).
public class DummyAtk_SpadeK : ICardEffect {
    public string Id => "DummyAtk_SpadeK";
    private bool _activeThisJudgment;
    private System.Action<NoteJudgedEvent> _handler;

    public void OnAttach(CardSystem ctx) {
        _handler = OnNoteJudged;
        EventBus.Subscribe(_handler);
    }
    public void OnDetach(CardSystem ctx) {
        if (_handler != null) EventBus.Unsubscribe(_handler);
        _handler = null;
        _activeThisJudgment = false;
    }
    private void OnNoteJudged(NoteJudgedEvent evt) {
        _activeThisJudgment = evt.Grade == JudgmentGrade.Perfect || evt.Grade == JudgmentGrade.Great;
    }
    public IEnumerable<ScoreModifier> GetCurrentModifiers() {
        if (_activeThisJudgment) yield return new ScoreModifier(1.10f, Id);
    }
    public bool TryConsumeComboProtection() => false;
}
