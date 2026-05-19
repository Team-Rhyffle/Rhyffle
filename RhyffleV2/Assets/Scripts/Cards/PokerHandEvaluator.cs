using System.Collections.Generic;
using System.Linq;

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
    public static PokerHand Evaluate(List<CardData> field) {
        if (field == null || field.Count < 2) return PokerHand.없음;

        var validCards = field.Where(c => c != null).ToList();
        if (validCards.Count < 2) return PokerHand.없음;

        // rank counts
        var rankCounts = new Dictionary<CardRank, int>();
        foreach (var c in validCards) {
            rankCounts.TryGetValue(c.Rank, out var n);
            rankCounts[c.Rank] = n + 1;
        }

        // suit counts
        var suitCounts = new Dictionary<CardSuit, int>();
        foreach (var c in validCards) {
            suitCounts.TryGetValue(c.Suit, out var n);
            suitCounts[c.Suit] = n + 1;
        }

        int quads = 0, triples = 0, pairs = 0;
        foreach (var kv in rankCounts) {
            if      (kv.Value >= 4) quads++;
            else if (kv.Value == 3) triples++;
            else if (kv.Value == 2) pairs++;
        }

        bool hasFlush = suitCounts.Values.Any(v => v >= 5);

        // straight: 5 consecutive ranks among the field cards (A=1 only)
        var distinctRanks = rankCounts.Keys
                                      .Select(r => (int)r)
                                      .Distinct()
                                      .OrderBy(x => x)
                                      .ToList();
        bool hasStraight = HasNConsecutive(distinctRanks, 5);

        // straight flush: 5 consecutive cards all of the same suit
        bool hasStraightFlush = false;
        if (hasFlush && hasStraight) {
            foreach (var kv in suitCounts) {
                if (kv.Value < 5) continue;
                var suitRanks = validCards
                                    .Where(c => c.Suit == kv.Key)
                                    .Select(c => (int)c.Rank)
                                    .Distinct()
                                    .OrderBy(x => x)
                                    .ToList();
                if (HasNConsecutive(suitRanks, 5)) { hasStraightFlush = true; break; }
            }
        }

        if (hasStraightFlush)              return PokerHand.스트레이트플러시;
        // TODO: Sprint 1.5.2 T2 — 럭키세븐
        if (quads > 0)                     return PokerHand.포카드;
        // TODO: Sprint 1.5.2 T2 — 더블트리플
        // TODO: Sprint 1.5.2 T2 — 레인보우
        if (triples >= 1 && pairs >= 1)    return PokerHand.풀하우스;
        // TODO: Sprint 1.5.2 T2 — 쓰리페어
        if (hasFlush)                      return PokerHand.플러시;
        if (hasStraight)                   return PokerHand.스트레이트;
        if (triples >= 1)                  return PokerHand.트리플;
        if (pairs >= 2)                    return PokerHand.투페어;
        if (pairs == 1)                    return PokerHand.원페어;
        return PokerHand.없음;
    }

    // Returns true if sortedDistinct contains at least n consecutive integers.
    private static bool HasNConsecutive(List<int> sortedDistinct, int n) {
        if (sortedDistinct.Count < n) return false;
        for (int start = 0; start <= sortedDistinct.Count - n; start++) {
            bool ok = true;
            for (int k = 1; k < n; k++) {
                if (sortedDistinct[start + k] != sortedDistinct[start] + k) { ok = false; break; }
            }
            if (ok) return true;
        }
        return false;
    }
}
