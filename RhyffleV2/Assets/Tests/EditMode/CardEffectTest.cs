using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

public class CardEffectTest {

    // ── DummyAtk_SpadeK ──────────────────────────────────────────────────────

    [Test]
    public void DummyAtk_Perfect_Yields1_10Modifier() {
        var atk = new DummyAtk_SpadeK();
        atk.OnAttach(null);
        try {
            EventBus.Publish(new NoteJudgedEvent { Grade = JudgmentGrade.Perfect, Modifiers = new List<ScoreModifier>() });
            var mods = atk.GetCurrentModifiers().ToList();
            Assert.AreEqual(1, mods.Count, "Perfect should yield exactly 1 modifier");
            Assert.AreEqual(1.10f, mods[0].Multiplier, 0.0001f, "Multiplier should be 1.10");
            Assert.AreEqual("DummyAtk_SpadeK", mods[0].Source);
        }
        finally {
            atk.OnDetach(null);
        }
    }

    [Test]
    public void DummyAtk_Great_Yields1_10Modifier() {
        var atk = new DummyAtk_SpadeK();
        atk.OnAttach(null);
        try {
            EventBus.Publish(new NoteJudgedEvent { Grade = JudgmentGrade.Great, Modifiers = new List<ScoreModifier>() });
            var mods = atk.GetCurrentModifiers().ToList();
            Assert.AreEqual(1, mods.Count, "Great should yield exactly 1 modifier");
            Assert.AreEqual(1.10f, mods[0].Multiplier, 0.0001f, "Multiplier should be 1.10");
        }
        finally {
            atk.OnDetach(null);
        }
    }

    [Test]
    public void DummyAtk_Good_YieldsNoModifier() {
        var atk = new DummyAtk_SpadeK();
        atk.OnAttach(null);
        try {
            EventBus.Publish(new NoteJudgedEvent { Grade = JudgmentGrade.Good, Modifiers = new List<ScoreModifier>() });
            var mods = atk.GetCurrentModifiers().ToList();
            Assert.AreEqual(0, mods.Count, "Good should yield no modifiers");
        }
        finally {
            atk.OnDetach(null);
        }
    }

    [Test]
    public void DummyAtk_Miss_YieldsNoModifier() {
        var atk = new DummyAtk_SpadeK();
        atk.OnAttach(null);
        try {
            EventBus.Publish(new NoteJudgedEvent { Grade = JudgmentGrade.Miss, Modifiers = new List<ScoreModifier>() });
            var mods = atk.GetCurrentModifiers().ToList();
            Assert.AreEqual(0, mods.Count, "Miss should yield no modifiers");
        }
        finally {
            atk.OnDetach(null);
        }
    }

    // ── DummyDef_HeartQ ──────────────────────────────────────────────────────

    [Test]
    public void DummyDef_MissWithChargeAvailable_TryConsumeReturnsTrueAndChargeDecreases() {
        var def = new DummyDef_HeartQ();
        def.OnAttach(null);
        try {
            EventBus.Publish(new NoteJudgedEvent { Grade = JudgmentGrade.Miss, Modifiers = new List<ScoreModifier>() });
            bool result = def.TryConsumeComboProtection();
            Assert.IsTrue(result, "TryConsumeComboProtection should return true when charge available and Miss occurred");
            // Second call in same cycle — charge already consumed, pending reset
            bool secondResult = def.TryConsumeComboProtection();
            Assert.IsFalse(secondResult, "Second TryConsume in same cycle should return false (pending already cleared)");
        }
        finally {
            def.OnDetach(null);
        }
    }

    [Test]
    public void DummyDef_MissWhenChargeDepleted_TryConsumeReturnsFalse() {
        var def = new DummyDef_HeartQ();
        def.OnAttach(null);
        try {
            // Deplete charge with first Miss
            EventBus.Publish(new NoteJudgedEvent { Grade = JudgmentGrade.Miss, Modifiers = new List<ScoreModifier>() });
            def.TryConsumeComboProtection();  // consume the only charge

            // Second Miss — no charges left
            EventBus.Publish(new NoteJudgedEvent { Grade = JudgmentGrade.Miss, Modifiers = new List<ScoreModifier>() });
            bool result = def.TryConsumeComboProtection();
            Assert.IsFalse(result, "TryConsumeComboProtection should return false when charges depleted");
        }
        finally {
            def.OnDetach(null);
        }
    }

    // ── DummySup_DiamondJ ────────────────────────────────────────────────────

    [Test]
    public void DummySup_FieldWithThreeSameSuit_Yields1_20Modifier() {
        var sup = new DummySup_DiamondJ();
        sup.OnAttach(null);
        try {
            var field = new List<CardData> {
                new CardData { Suit = CardSuit.Spade },
                new CardData { Suit = CardSuit.Spade },
                new CardData { Suit = CardSuit.Spade },
            };
            EventBus.Publish(new FieldChangedEvent { Field = field });
            var mods = sup.GetCurrentModifiers().ToList();
            Assert.AreEqual(1, mods.Count, "3 same-suit cards should yield exactly 1 modifier");
            Assert.AreEqual(1.20f, mods[0].Multiplier, 0.0001f, "Multiplier should be 1.20");
            Assert.AreEqual("DummySup_DiamondJ", mods[0].Source);
        }
        finally {
            sup.OnDetach(null);
        }
    }

    [Test]
    public void DummySup_FieldMixedSuits_YieldsNoModifier() {
        var sup = new DummySup_DiamondJ();
        sup.OnAttach(null);
        try {
            var field = new List<CardData> {
                new CardData { Suit = CardSuit.Spade },
                new CardData { Suit = CardSuit.Heart },
                new CardData { Suit = CardSuit.Diamond },
            };
            EventBus.Publish(new FieldChangedEvent { Field = field });
            var mods = sup.GetCurrentModifiers().ToList();
            Assert.AreEqual(0, mods.Count, "Mixed suits (all different) should yield no modifiers");
        }
        finally {
            sup.OnDetach(null);
        }
    }
}
