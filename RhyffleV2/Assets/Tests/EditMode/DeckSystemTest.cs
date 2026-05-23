using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DeckSystemTest {
    private GameObject _go;
    private DeckSystem _deck;

    [SetUp]
    public void SetUp() {
        _go = new GameObject("DeckSystemTest");
        _deck = _go.AddComponent<DeckSystem>();
        // EditMode NUnit: Awake 자동 호출 안 됨 → InitDeck() 명시 호출
        _deck.InitDeck();
    }

    [TearDown]
    public void TearDown() {
        Object.DestroyImmediate(_go);
    }

    [Test]
    public void Awake_SeedsDummyDeck_AndPopulatesDrawPile() {
        // 24장 dummy seed → draw pile 24장
        Assert.AreEqual(DeckSystem.DUMMY_DECK_SIZE, _deck.DeckCount);
        Assert.AreEqual(0, _deck.GraveyardCount);
    }

    [Test]
    public void DrawN_RemovesFromPile_AndReturnsRequestedCount() {
        var drawn = _deck.DrawN(8);
        Assert.AreEqual(8, drawn.Count);
        Assert.AreEqual(DeckSystem.DUMMY_DECK_SIZE - 8, _deck.DeckCount);
        // 각 카드 non-null
        foreach (var c in drawn) Assert.NotNull(c);
    }

    [Test]
    public void ShuffleAll_MergesGraveyardIntoDrawPile() {
        // 8장 draw + graveyard로 직접 push (test only)
        var drawn = _deck.DrawN(8);
        foreach (var c in drawn) _deck.Graveyard_TestOnly.Enqueue(c);
        Assert.AreEqual(DeckSystem.DUMMY_DECK_SIZE - 8, _deck.DeckCount);
        Assert.AreEqual(8, _deck.GraveyardCount);
        _deck.ForceShuffleAll_TestOnly();
        Assert.AreEqual(DeckSystem.DUMMY_DECK_SIZE, _deck.DeckCount);  // 합쳐서 24장
        Assert.AreEqual(0, _deck.GraveyardCount);
    }

    [Test]
    public void SetSource_BeforeInit_InjectsCustomDeck() {
        // 새 DeckSystem (SetUp의 InitDeck 무시하고 fresh로)
        var go2 = new GameObject("CustomSource");
        var deck2 = go2.AddComponent<DeckSystem>();
        deck2.SetSource(new FixedThreeCardSource());
        deck2.InitDeck();
        Assert.AreEqual(3, deck2.DeckCount);
        Object.DestroyImmediate(go2);
    }

    private class FixedThreeCardSource : IDeckSource {
        public List<CardData> LoadDeck() {
            return new List<CardData> {
                new CardData { Id = "Fixed_00", Suit = CardSuit.Spade, Rank = CardRank.A },
                new CardData { Id = "Fixed_01", Suit = CardSuit.Heart, Rank = CardRank.K },
                new CardData { Id = "Fixed_02", Suit = CardSuit.Club,  Rank = CardRank.Q },
            };
        }
    }

    [Test]
    public void OnKeyNote_ReplacesFieldAndSendsOldToGraveyard() {
        // CardSystem mock attach
        var csGO = new GameObject("CardSystemMock");
        var cs = csGO.AddComponent<CardSystem>();
        _deck.cardSystem = cs;
        // 초기 8장 field 채우기
        var initial = _deck.DrawN(8);
        cs.SetField(initial);
        Assert.AreEqual(8, cs.GetField().Count);
        Assert.AreEqual(DeckSystem.DUMMY_DECK_SIZE - 8, _deck.DeckCount);
        Assert.AreEqual(0, _deck.GraveyardCount);
        // 키노트 trigger
        _deck.TriggerKeyNote_TestOnly();
        // field 8장 새로 채워짐 + 묘지 8장
        Assert.AreEqual(8, cs.GetField().Count);
        Assert.AreEqual(8, _deck.GraveyardCount);
        Object.DestroyImmediate(csGO);
    }
}
