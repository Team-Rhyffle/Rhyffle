using System.Collections.Generic;

public struct NoteJudgedEvent {
    public JudgmentGrade       Grade;
    public NoteKind            Kind;
    public int                 Line;
    public int                 Combo;
    public int                 BaseScore;
    public List<ScoreModifier> Modifiers;   // GameLoop reads after CardSystem populates
}
