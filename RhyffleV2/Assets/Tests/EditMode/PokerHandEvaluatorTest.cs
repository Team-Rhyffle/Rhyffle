using NUnit.Framework;
using System.Collections.Generic;

public class PokerHandEvaluatorTest {
    // Helper: build a CardData inline
    static CardData C(CardSuit s, CardRank r) => new CardData { Suit = s, Rank = r };

    // ── existing tests (updated) ─────────────────────────────────────────────

    [Test]
    public void Evaluate_SameRankTwoCards_Returns원페어() {
        var field = new List<CardData> {
            C(CardSuit.Spade, CardRank.A),
            C(CardSuit.Heart, CardRank.A),
        };
        Assert.AreEqual(PokerHand.원페어, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_AllDistinctRanks_Returns없음() {
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.R2),
            C(CardSuit.Diamond, CardRank.R3),
        };
        Assert.AreEqual(PokerHand.없음, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_ThreeSameRank_ReturnsTriple() {
        // Three Kings → 트리플 (stub test renamed: 트리플 detection now implemented)
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.K),
            C(CardSuit.Heart,   CardRank.K),
            C(CardSuit.Diamond, CardRank.K),
        };
        Assert.AreEqual(PokerHand.트리플, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void PokerHandEnum_HasAll13Values() {
        // 없음 + 12 hand types = 13 enum values
        var values = System.Enum.GetValues(typeof(PokerHand));
        Assert.AreEqual(13, values.Length);
        // Spot-check key values
        Assert.AreEqual(0,  (int)PokerHand.없음);
        Assert.AreEqual(1,  (int)PokerHand.원페어);
        Assert.AreEqual(12, (int)PokerHand.스트레이트플러시);
        // Verify all expected names exist
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "투페어"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "트리플"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "스트레이트"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "플러시"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "쓰리페어"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "풀하우스"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "레인보우"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "더블트리플"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "포카드"));
        Assert.IsTrue(System.Enum.IsDefined(typeof(PokerHand), "럭키세븐"));
    }

    // ── new tests: 7 traditional hands ──────────────────────────────────────

    [Test]
    public void Evaluate_FourSameRank_Returns포카드() {
        // AAAA + K → 포카드
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.A),
            C(CardSuit.Diamond, CardRank.A),
            C(CardSuit.Club,    CardRank.A),
            C(CardSuit.Spade,   CardRank.K),
        };
        Assert.AreEqual(PokerHand.포카드, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_TripleAndPair_Returns풀하우스() {
        // AAA + KK → 풀하우스
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.A),
            C(CardSuit.Diamond, CardRank.A),
            C(CardSuit.Spade,   CardRank.K),
            C(CardSuit.Heart,   CardRank.K),
        };
        Assert.AreEqual(PokerHand.풀하우스, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_TripleAlone_Returns트리플() {
        // AAA + K + Q (no pair) → 트리플
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.A),
            C(CardSuit.Diamond, CardRank.A),
            C(CardSuit.Club,    CardRank.K),
            C(CardSuit.Spade,   CardRank.Q),
        };
        Assert.AreEqual(PokerHand.트리플, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_TwoPairs_Returns투페어() {
        // AA + KK + Q → 투페어
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.A),
            C(CardSuit.Diamond, CardRank.K),
            C(CardSuit.Club,    CardRank.K),
            C(CardSuit.Spade,   CardRank.Q),
        };
        Assert.AreEqual(PokerHand.투페어, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_FiveSameSuit_Returns플러시() {
        // 5 Spades, all distinct ranks → 플러시 (not a straight)
        var field = new List<CardData> {
            C(CardSuit.Spade, CardRank.A),
            C(CardSuit.Spade, CardRank.R3),
            C(CardSuit.Spade, CardRank.R6),
            C(CardSuit.Spade, CardRank.R9),
            C(CardSuit.Spade, CardRank.Q),
        };
        Assert.AreEqual(PokerHand.플러시, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_FiveConsecutiveRanks_Returns스트레이트() {
        // 2-3-4-5-6, mixed suits → 스트레이트
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.R2),
            C(CardSuit.Heart,   CardRank.R3),
            C(CardSuit.Diamond, CardRank.R4),
            C(CardSuit.Club,    CardRank.R5),
            C(CardSuit.Spade,   CardRank.R6),
        };
        Assert.AreEqual(PokerHand.스트레이트, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_StraightFlush_Returns스트레이트플러시() {
        // 2♠-3♠-4♠-5♠-6♠ → 스트레이트플러시
        var field = new List<CardData> {
            C(CardSuit.Spade, CardRank.R2),
            C(CardSuit.Spade, CardRank.R3),
            C(CardSuit.Spade, CardRank.R4),
            C(CardSuit.Spade, CardRank.R5),
            C(CardSuit.Spade, CardRank.R6),
        };
        Assert.AreEqual(PokerHand.스트레이트플러시, PokerHandEvaluator.Evaluate(field));
    }

    // ── priority tests ───────────────────────────────────────────────────────

    [Test]
    public void Evaluate_FullHouseVsTriple_Priority풀하우스Wins() {
        // AAA + KK qualifies both 풀하우스 and 트리플; 풀하우스 must win
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.A),
            C(CardSuit.Diamond, CardRank.A),
            C(CardSuit.Spade,   CardRank.K),
            C(CardSuit.Heart,   CardRank.K),
        };
        Assert.AreEqual(PokerHand.풀하우스, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_StraightFlushVsFlush_Priority스트레이트플러시Wins() {
        // 5 consecutive same-suit cards qualifies both 플러시 and 스트레이트플러시;
        // 스트레이트플러시 must win
        var field = new List<CardData> {
            C(CardSuit.Heart, CardRank.R3),
            C(CardSuit.Heart, CardRank.R4),
            C(CardSuit.Heart, CardRank.R5),
            C(CardSuit.Heart, CardRank.R6),
            C(CardSuit.Heart, CardRank.R7),
        };
        Assert.AreEqual(PokerHand.스트레이트플러시, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_StraightFlushVsStraight_Priority스트레이트플러시Wins() {
        // Same hand as above — also qualifies as 스트레이트; 스트레이트플러시 must win
        var field = new List<CardData> {
            C(CardSuit.Diamond, CardRank.R7),
            C(CardSuit.Diamond, CardRank.R8),
            C(CardSuit.Diamond, CardRank.R9),
            C(CardSuit.Diamond, CardRank.R10),
            C(CardSuit.Diamond, CardRank.J),
        };
        Assert.AreEqual(PokerHand.스트레이트플러시, PokerHandEvaluator.Evaluate(field));
    }

    // ── Sprint 1.5.2 T2: 4 Rhyffle-specific hands ───────────────────────────

    [Test]
    public void Evaluate_SevenConsecutiveRanks_Returns럭키세븐() {
        // ranks 1..7 mixed suits → 럭키세븐 (not 스트레이트플러시 since suits differ)
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.R2),
            C(CardSuit.Diamond, CardRank.R3),
            C(CardSuit.Club,    CardRank.R4),
            C(CardSuit.Spade,   CardRank.R5),
            C(CardSuit.Heart,   CardRank.R6),
            C(CardSuit.Diamond, CardRank.R7),
        };
        Assert.AreEqual(PokerHand.럭키세븐, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_SevenConsecutiveSameSuit_Returns스트레이트플러시() {
        // all Spades 1..7 — 5 consec same suit fires first → 스트레이트플러시 (higher priority)
        var field = new List<CardData> {
            C(CardSuit.Spade, CardRank.A),
            C(CardSuit.Spade, CardRank.R2),
            C(CardSuit.Spade, CardRank.R3),
            C(CardSuit.Spade, CardRank.R4),
            C(CardSuit.Spade, CardRank.R5),
            C(CardSuit.Spade, CardRank.R6),
            C(CardSuit.Spade, CardRank.R7),
        };
        Assert.AreEqual(PokerHand.스트레이트플러시, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_TwoTriples_Returns더블트리플() {
        // AAA + KKK + Q
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),   C(CardSuit.Heart,   CardRank.A),   C(CardSuit.Diamond, CardRank.A),
            C(CardSuit.Club,    CardRank.K),   C(CardSuit.Spade,   CardRank.K),   C(CardSuit.Heart,   CardRank.K),
            C(CardSuit.Diamond, CardRank.Q),
        };
        Assert.AreEqual(PokerHand.더블트리플, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_FourSuitsAllPresent_Returns레인보우() {
        // 4 suits each at least once, no pair/triple, non-consecutive ranks
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.R3),
            C(CardSuit.Diamond, CardRank.R5),
            C(CardSuit.Club,    CardRank.R7),
            C(CardSuit.Spade,   CardRank.R9),
            C(CardSuit.Heart,   CardRank.J),
            C(CardSuit.Diamond, CardRank.K),
        };
        Assert.AreEqual(PokerHand.레인보우, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_ThreePairs_Returns쓰리페어() {
        // AA + KK + QQ + J (no triple, only 2 suits → no 레인보우)
        var field = new List<CardData> {
            C(CardSuit.Spade, CardRank.A),  C(CardSuit.Heart, CardRank.A),
            C(CardSuit.Spade, CardRank.K),  C(CardSuit.Heart, CardRank.K),
            C(CardSuit.Spade, CardRank.Q),  C(CardSuit.Heart, CardRank.Q),
            C(CardSuit.Spade, CardRank.J),
        };
        Assert.AreEqual(PokerHand.쓰리페어, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_더블트리플VsPokar_Priority포카드Wins() {
        // AAAA + KKK — quad takes priority over double-triple (10 > 9)
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A), C(CardSuit.Heart,   CardRank.A),
            C(CardSuit.Diamond, CardRank.A), C(CardSuit.Club,    CardRank.A),
            C(CardSuit.Spade,   CardRank.K), C(CardSuit.Heart,   CardRank.K), C(CardSuit.Diamond, CardRank.K),
        };
        Assert.AreEqual(PokerHand.포카드, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_레인보우VsFullhouse_Priority레인보우Wins() {
        // AAA + KK + Q + J with all 4 suits present → 레인보우(8) beats 풀하우스(7)
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A),
            C(CardSuit.Heart,   CardRank.A),
            C(CardSuit.Diamond, CardRank.A),
            C(CardSuit.Club,    CardRank.K),
            C(CardSuit.Spade,   CardRank.K),
            C(CardSuit.Heart,   CardRank.Q),
            C(CardSuit.Diamond, CardRank.J),
        };
        Assert.AreEqual(PokerHand.레인보우, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_쓰리페어VsFullhouse_Priority풀하우스Wins() {
        // AAA + KK + QQ — triples=1, pairs=2 → 풀하우스 wins over 쓰리페어
        // Only 3 suits used (no Club) → 레인보우 FALSE
        var field = new List<CardData> {
            C(CardSuit.Spade,   CardRank.A), C(CardSuit.Heart,   CardRank.A), C(CardSuit.Diamond, CardRank.A),
            C(CardSuit.Spade,   CardRank.K), C(CardSuit.Heart,   CardRank.K),
            C(CardSuit.Spade,   CardRank.Q), C(CardSuit.Heart,   CardRank.Q),
        };
        Assert.AreEqual(PokerHand.풀하우스, PokerHandEvaluator.Evaluate(field));
    }
}
