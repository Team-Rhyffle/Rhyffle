using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprint 1.5.6 더미 덱 — 24장 random Suit/Rank. 카드 데이터셋 도착 전 placeholder.
/// Sprint 2에서 JsonDeckSource 로 교체 (한울+동욱 카드 DB JSON 입력 자산).
///
/// DeckSystem 의 default IDeckSource. 테스트에서 DeckSystem.SetSource(...) 로 교체 가능.
/// </summary>
public class DummyDeckSource : IDeckSource {
    public List<CardData> LoadDeck() {
        var deck  = new List<CardData>();
        var suits = (CardSuit[])System.Enum.GetValues(typeof(CardSuit));
        var ranks = (CardRank[])System.Enum.GetValues(typeof(CardRank));
        for (int i = 0; i < DeckSystem.DUMMY_DECK_SIZE; i++) {
            deck.Add(new CardData {
                Id    = $"Dummy_{i:D2}",
                Suit  = suits[Random.Range(0, suits.Length)],
                Rank  = ranks[Random.Range(0, ranks.Length)],
                Group = CardGroup.공격형,
            });
        }
        return deck;
    }
}
