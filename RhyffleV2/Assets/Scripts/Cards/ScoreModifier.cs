public struct ScoreModifier {
    public float  Multiplier;   // e.g. 1.10f = +10%
    public string Source;       // e.g. "DummyAtk_SpadeK" — for debug/log

    public ScoreModifier(float multiplier, string source) {
        Multiplier = multiplier;
        Source     = source;
    }

    public override string ToString() => $"{Source}×{Multiplier:0.00}";
}
