public class CardData {
    public string     Id;                 // ex "DummyAtk_SpadeK"
    public CardSuit   Suit;
    public CardRank   Rank;
    public CardRarity Rarity;
    public CardGroup  Group;
    public string     UniqueAbilityId;    // null/empty if 없음
    public string     GeneralAbilityId;   // null/empty if 미발급
    public bool       DoubleSided;        // 양면 flag
}
