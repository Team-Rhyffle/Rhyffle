using NUnit.Framework;

public class CardMetadataTest {
    [Test]
    public void Key_ComposesArtistSeasonMemberCollectionNo() {
        var meta = new CardMetadata {
            Artist       = "tripleS",
            Season       = "Atom01",
            Member       = "ChaeYeon",
            CollectionNo = "101Z",
        };
        Assert.AreEqual("tripleS Atom01 ChaeYeon 101Z", meta.Key);
    }

    [Test]
    public void AllFields_AreAssignableAndReadable() {
        var meta = new CardMetadata {
            ImageFile        = "ARTMS_Divine01_Heejin_120Z.jpg",
            Artist           = "ARTMS",
            Member           = "Heejin",
            Season           = "Divine01",
            CollectionNo     = "120Z",
            Class            = "Special",
            Suit             = CardSuit.Spade,
            Rank             = CardRank.K,
            Rarity           = CardRarity.Epic,
            Group            = CardGroup.공격형,
            DoubleSided      = true,
            UniqueAbilityId  = "장미전쟁_K",
        };
        Assert.AreEqual("ARTMS_Divine01_Heejin_120Z.jpg", meta.ImageFile);
        Assert.AreEqual("Special",                       meta.Class);
        Assert.AreEqual(CardSuit.Spade,                  meta.Suit);
        Assert.IsTrue(meta.DoubleSided);
    }
}
