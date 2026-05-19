using System.Collections.Generic;

// 수비형 더미 — Miss 발생 시 콤보 1회 보호 (총 1 charge, 사용 후 소진).
// Pattern: on Miss event, mark _pendingProtect=true (only if charges remain).
// GameLoop then calls TryConsumeComboProtection — we consume charge and return true.
// Non-Miss judgments reset _pendingProtect to false.
public class DummyDef_HeartQ : ICardEffect {
    public const int INITIAL_CHARGES = 1;
    public string Id => "DummyDef_HeartQ";
    private int _charges = INITIAL_CHARGES;
    private bool _pendingProtect;
    private System.Action<NoteJudgedEvent> _handler;

    public void OnAttach(CardSystem ctx) {
        _handler = OnNoteJudged;
        EventBus.Subscribe(_handler);
    }
    public void OnDetach(CardSystem ctx) {
        if (_handler != null) EventBus.Unsubscribe(_handler);
        _handler = null;
        _pendingProtect = false;
    }
    private void OnNoteJudged(NoteJudgedEvent evt) {
        _pendingProtect = (evt.Grade == JudgmentGrade.Miss) && _charges > 0;
    }
    public IEnumerable<ScoreModifier> GetCurrentModifiers() {
        yield break;   // def doesn't multiply
    }
    public bool TryConsumeComboProtection() {
        if (!_pendingProtect) return false;
        _charges--;
        _pendingProtect = false;
        return true;
    }
}
