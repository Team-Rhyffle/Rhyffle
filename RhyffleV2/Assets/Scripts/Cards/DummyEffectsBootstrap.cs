using UnityEngine;

// Sprint 1.5: hardcoded registration of 3 dummy effects. Replace with
// JSON loader + 김한울 dataset in Sprint 2.
public static class DummyEffectsBootstrap {
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Register() {
        CardEffectRegistry.Register("DummyAtk_SpadeK",  () => new DummyAtk_SpadeK());
        CardEffectRegistry.Register("DummyDef_HeartQ",  () => new DummyDef_HeartQ());
        CardEffectRegistry.Register("DummySup_DiamondJ", () => new DummySup_DiamondJ());
    }
}
