using NUnit.Framework;

public class CardMetadataRegistryTest {
    private static CardMetadata Meta(string artist, string season, string member, string col) {
        return new CardMetadata {
            Artist = artist, Season = season, Member = member, CollectionNo = col, Class = "First",
        };
    }

    [Test]
    public void Add_StoresByKey_AndGetReturnsSameInstance() {
        var registry = new CardMetadataRegistry();
        var m1 = Meta("tripleS", "Atom01", "ChaeYeon", "101Z");
        registry.Add(m1);
        var got = registry.Get("tripleS Atom01 ChaeYeon 101Z");
        Assert.AreSame(m1, got);
    }

    [Test]
    public void Get_MissingKey_ReturnsNull() {
        var registry = new CardMetadataRegistry();
        Assert.IsNull(registry.Get("nonexistent key"));
    }

    [Test]
    public void Count_ReflectsAddedEntries() {
        var registry = new CardMetadataRegistry();
        Assert.AreEqual(0, registry.Count);
        registry.Add(Meta("tripleS", "Atom01", "ChaeYeon", "101Z"));
        registry.Add(Meta("ARTMS",   "Divine01", "Heejin",  "120Z"));
        Assert.AreEqual(2, registry.Count);
    }

    [Test]
    public void Add_DuplicateKey_OverwritesPrevious() {
        var registry = new CardMetadataRegistry();
        var first  = Meta("tripleS", "Atom01", "ChaeYeon", "101Z");
        first.Class = "First";
        var second = Meta("tripleS", "Atom01", "ChaeYeon", "101Z");
        second.Class = "Special";
        registry.Add(first);
        registry.Add(second);
        Assert.AreEqual(1, registry.Count);
        Assert.AreEqual("Special", registry.Get(first.Key).Class);
    }

    [Test]
    public void GetByInstance_LooksUpViaObjektName() {
        var registry = new CardMetadataRegistry();
        var meta = Meta("tripleS", "Atom01", "ChaeYeon", "101Z");
        registry.Add(meta);
        var instance = new CardInstance { ObjektName = meta.Key };
        Assert.AreSame(meta, registry.GetByInstance(instance));
    }

    [Test]
    public void GetByInstance_NullInstanceOrUnknownKey_ReturnsNull() {
        var registry = new CardMetadataRegistry();
        Assert.IsNull(registry.GetByInstance(null));
        Assert.IsNull(registry.GetByInstance(new CardInstance { ObjektName = "missing" }));
    }
}
