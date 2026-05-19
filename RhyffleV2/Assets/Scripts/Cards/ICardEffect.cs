using System.Collections.Generic;

public interface ICardEffect {
    string Id { get; }
    void OnAttach(CardSystem ctx);   // card subscribes to events via ctx
    void OnDetach(CardSystem ctx);   // card unsubscribes

    // Modifiers this effect currently contributes. Empty if inactive.
    // Called every NoteJudgedEvent dispatch — keep it allocation-light if possible.
    IEnumerable<ScoreModifier> GetCurrentModifiers();

    // Returns true if this effect chose to consume a combo-protection charge
    // for the current Miss. CardSystem queries effects left→right; first true wins.
    bool TryConsumeComboProtection();
}
