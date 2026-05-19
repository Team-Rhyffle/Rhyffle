using System.Collections.Generic;

public enum PokerHand {
    없음 = 0,                  // None — no hand detected
    원페어 = 1,                // One pair (same rank ×2)
    투페어 = 2,                // Two pair
    트리플 = 3,                // Three of a kind
    스트레이트 = 4,            // Straight
    플러시 = 5,                // Flush (same suit)
    쓰리페어 = 6,              // Three pair (Rhyffle new)
    풀하우스 = 7,              // Full house
    레인보우 = 8,              // Rainbow — all 4 suits (Rhyffle new)
    더블트리플 = 9,            // Two triples (Rhyffle new)
    포카드 = 10,               // Four of a kind
    럭키세븐 = 11,             // Lucky seven (Rhyffle new — definition TBD)
    스트레이트플러시 = 12,     // Straight flush
}

public static class PokerHandEvaluator {
    // Sprint 1.5 stub: only 원페어 (one pair) is actually detected.
    // All other hand recognition arrives with Sprint 2 spec finalization.
    public static PokerHand Evaluate(List<CardData> field) {
        if (field == null || field.Count < 2) return PokerHand.없음;
        var rankCounts = new Dictionary<CardRank, int>();
        foreach (var card in field) {
            if (card == null) continue;
            rankCounts.TryGetValue(card.Rank, out var n);
            rankCounts[card.Rank] = n + 1;
        }
        foreach (var count in rankCounts.Values) {
            if (count >= 2) return PokerHand.원페어;
        }
        return PokerHand.없음;
    }
}
