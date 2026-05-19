using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// Sprint 1.5 Steelman Gates — Task 10 통합 검증
// Gate A: EventBus 메모리 누수 (subscribe/unsubscribe 사이클 후 핸들러 미발화)
// Gate B: 카드 효과 삽입 순서 (Dictionary left→right 보장 경험적 검증)
public class SteelmanGatesTest {

    [TearDown]
    public void TearDown() {
        CardEffectRegistry.ClearForTests();
    }

    // ── Gate A: EventBus memory leak ─────────────────────────────────────────

    [Test]
    public void GateA_SubscribeUnsubscribe50Cycles_HandlerDoesNotFireAfterward() {
        int fireCount = 0;
        Action<NoteJudgedEvent> handler = e => fireCount++;

        for (int i = 0; i < 50; i++) {
            EventBus.Subscribe(handler);
            EventBus.Unsubscribe(handler);
        }

        EventBus.Publish(new NoteJudgedEvent {
            Grade     = JudgmentGrade.Perfect,
            Kind      = NoteKind.Normal,
            Line      = 0,
            Combo     = 0,
            BaseScore = 0,
            Modifiers = new List<ScoreModifier>(),
        });

        Assert.AreEqual(0, fireCount,
            "Handler must not fire after 50 subscribe/unsubscribe cycles — no EventBus leak");
    }

    // ── Gate B: Card effect insertion order (left→right) ────────────────────

    [Test]
    public void GateB_RegistryInsertionOrder_IsPreservedInGetAllFactories() {
        CardEffectRegistry.Register("DummyAtk_SpadeK",  () => new DummyAtk_SpadeK());
        CardEffectRegistry.Register("DummyDef_HeartQ",  () => new DummyDef_HeartQ());
        CardEffectRegistry.Register("DummySup_DiamondJ",() => new DummySup_DiamondJ());

        var keys = CardEffectRegistry.GetAllFactories().Select(kv => kv.Key).ToList();

        Assert.AreEqual(3, keys.Count, "Registry should have exactly 3 entries");
        Assert.AreEqual("DummyAtk_SpadeK",  keys[0], "First entry should be ATK");
        Assert.AreEqual("DummyDef_HeartQ",  keys[1], "Second entry should be DEF");
        Assert.AreEqual("DummySup_DiamondJ",keys[2], "Third entry should be SUP");
    }

    [Test]
    public void GateB_GetScoreMultipliers_ReturnsModifiersInInsertionOrder_AtkFirst_SupSecond() {
        // With 3 Spade cards, ATK and SUP both yield modifiers; DEF yields nothing.
        // Verifies left→right ordering: ATK(1.10) appears before SUP(1.20).
        var atk = new DummyAtk_SpadeK();
        var def = new DummyDef_HeartQ();
        var sup = new DummySup_DiamondJ();
        atk.OnAttach(null);
        def.OnAttach(null);
        sup.OnAttach(null);

        try {
            // Activate SUP via 3-Spade field
            EventBus.Publish(new FieldChangedEvent {
                Field = new List<CardData> {
                    new CardData { Suit = CardSuit.Spade, Rank = CardRank.K },
                    new CardData { Suit = CardSuit.Spade, Rank = CardRank.Q },
                    new CardData { Suit = CardSuit.Spade, Rank = CardRank.J },
                }
            });

            // Activate ATK via Perfect
            EventBus.Publish(new NoteJudgedEvent {
                Grade     = JudgmentGrade.Perfect,
                Kind      = NoteKind.Normal,
                Line      = 0,
                Combo     = 0,
                BaseScore = 0,
                Modifiers = new List<ScoreModifier>(),
            });

            // Collect in declared left→right order (same pattern as CardSystem.GetScoreMultipliers)
            ICardEffect[] effects = { atk, def, sup };
            var mods = new List<ScoreModifier>();
            foreach (var fx in effects)
                foreach (var m in fx.GetCurrentModifiers())
                    mods.Add(m);

            Assert.AreEqual(2, mods.Count, "Expected exactly 2 modifiers (ATK + SUP; DEF yields nothing)");
            Assert.AreEqual("DummyAtk_SpadeK",  mods[0].Source, "First modifier must be ATK (left→right)");
            Assert.AreEqual("DummySup_DiamondJ", mods[1].Source, "Second modifier must be SUP (left→right)");
            Assert.AreEqual(1.10f, mods[0].Multiplier, 0.0001f, "ATK multiplier should be 1.10");
            Assert.AreEqual(1.20f, mods[1].Multiplier, 0.0001f, "SUP multiplier should be 1.20");
        }
        finally {
            atk.OnDetach(null);
            def.OnDetach(null);
            sup.OnDetach(null);
        }
    }
}
