using NUnit.Framework;
using System.Collections.Generic;

public class PokerHandEvaluatorTest {
    [Test]
    public void Evaluate_SameRankTwoCards_Returns원페어() {
        // Two Spade Aces → 원페어
        var field = new List<CardData> {
            new CardData { Suit = CardSuit.Spade, Rank = CardRank.A },
            new CardData { Suit = CardSuit.Heart, Rank = CardRank.A },
        };
        Assert.AreEqual(PokerHand.원페어, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_AllDistinctRanks_Returns없음() {
        var field = new List<CardData> {
            new CardData { Suit = CardSuit.Spade, Rank = CardRank.A },
            new CardData { Suit = CardSuit.Heart, Rank = CardRank.R2 },
            new CardData { Suit = CardSuit.Diamond, Rank = CardRank.R3 },
        };
        Assert.AreEqual(PokerHand.없음, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void Evaluate_ThreeSameRank_StillReturns원페어AsStub() {
        // Sprint 1.5 stub: 3-of-a-kind returns 원페어 since 트리플 detection unimplemented
        var field = new List<CardData> {
            new CardData { Suit = CardSuit.Spade, Rank = CardRank.K },
            new CardData { Suit = CardSuit.Heart, Rank = CardRank.K },
            new CardData { Suit = CardSuit.Diamond, Rank = CardRank.K },
        };
        Assert.AreEqual(PokerHand.원페어, PokerHandEvaluator.Evaluate(field));
    }

    [Test]
    public void PokerHandEnum_HasAll13Values() {
        // 없음 + 12 hand types = 13 enum values
        var values = System.Enum.GetValues(typeof(PokerHand));
        Assert.AreEqual(13, values.Length);
        // Spot-check key values
        Assert.AreEqual(0, (int)PokerHand.없음);
        Assert.AreEqual(1, (int)PokerHand.원페어);
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
}
