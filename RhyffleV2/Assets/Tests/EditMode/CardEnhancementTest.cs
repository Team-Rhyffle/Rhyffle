using NUnit.Framework;

public class CardEnhancementTest {
    private static CardMetadata Meta(string artist, string member, string cls) {
        return new CardMetadata {
            Artist = artist,
            Member = member,
            Class  = cls,
            Season       = "Atom01",
            CollectionNo = "101Z",
        };
    }

    // === Harmony 검증: 동일 아티스트 / 동일 등급 / 다른 멤버 ===
    [Test]
    public void ValidateHarmony_SameArtistSameClassDifferentMember_Succeeds() {
        var t = Meta("tripleS", "ChaeYeon", "Special");
        var m = Meta("tripleS", "SeoYeon",  "Special");
        Assert.AreEqual(CardEnhancement.EnhanceResult.Success,
                        CardEnhancement.ValidateHarmonyMaterial(t, m));
    }

    [Test]
    public void ValidateHarmony_SameMember_Fails() {
        var t = Meta("tripleS", "ChaeYeon", "Special");
        var m = Meta("tripleS", "ChaeYeon", "Special");
        Assert.AreEqual(CardEnhancement.EnhanceResult.IncompatibleMaterial,
                        CardEnhancement.ValidateHarmonyMaterial(t, m));
    }

    [Test]
    public void ValidateHarmony_DifferentArtist_Fails() {
        var t = Meta("tripleS", "ChaeYeon", "Special");
        var m = Meta("ARTMS",   "Heejin",   "Special");
        Assert.AreEqual(CardEnhancement.EnhanceResult.IncompatibleMaterial,
                        CardEnhancement.ValidateHarmonyMaterial(t, m));
    }

    [Test]
    public void ValidateHarmony_DifferentClass_Fails() {
        var t = Meta("tripleS", "ChaeYeon", "Special");
        var m = Meta("tripleS", "SeoYeon",  "Double");
        Assert.AreEqual(CardEnhancement.EnhanceResult.IncompatibleMaterial,
                        CardEnhancement.ValidateHarmonyMaterial(t, m));
    }

    // === Resonance 검증: 동일 아티스트 / 동일 등급 / 동일 멤버 ===
    [Test]
    public void ValidateResonance_SameArtistSameClassSameMember_Succeeds() {
        var t = Meta("tripleS", "ChaeYeon", "Special");
        var m = Meta("tripleS", "ChaeYeon", "Special");
        Assert.AreEqual(CardEnhancement.EnhanceResult.Success,
                        CardEnhancement.ValidateResonanceMaterial(t, m));
    }

    [Test]
    public void ValidateResonance_DifferentMember_Fails() {
        var t = Meta("tripleS", "ChaeYeon", "Special");
        var m = Meta("tripleS", "SeoYeon",  "Special");
        Assert.AreEqual(CardEnhancement.EnhanceResult.IncompatibleMaterial,
                        CardEnhancement.ValidateResonanceMaterial(t, m));
    }

    // === Apply: level 증가 + cap (10) ===
    [Test]
    public void ApplyHarmony_ValidMaterial_IncrementsLevel() {
        var t  = Meta("tripleS", "ChaeYeon", "Special");
        var m  = Meta("tripleS", "SeoYeon",  "Special");
        var ci = new CardInstance { HarmonyLevel = 3 };
        var r  = CardEnhancement.ApplyHarmony(ci, t, m);
        Assert.AreEqual(CardEnhancement.EnhanceResult.Success, r);
        Assert.AreEqual(4, ci.HarmonyLevel);
    }

    [Test]
    public void ApplyHarmony_AtMaxLevel_FailsWithoutIncrement() {
        var t  = Meta("tripleS", "ChaeYeon", "Special");
        var m  = Meta("tripleS", "SeoYeon",  "Special");
        var ci = new CardInstance { HarmonyLevel = CardInstance.MAX_ENHANCEMENT_LEVEL };
        var r  = CardEnhancement.ApplyHarmony(ci, t, m);
        Assert.AreEqual(CardEnhancement.EnhanceResult.AtMaxLevel, r);
        Assert.AreEqual(CardInstance.MAX_ENHANCEMENT_LEVEL, ci.HarmonyLevel);
    }

    [Test]
    public void ApplyHarmony_IncompatibleMaterial_FailsWithoutIncrement() {
        var t  = Meta("tripleS", "ChaeYeon", "Special");
        var m  = Meta("ARTMS",   "Heejin",   "Special");
        var ci = new CardInstance { HarmonyLevel = 5 };
        var r  = CardEnhancement.ApplyHarmony(ci, t, m);
        Assert.AreEqual(CardEnhancement.EnhanceResult.IncompatibleMaterial, r);
        Assert.AreEqual(5, ci.HarmonyLevel);
    }

    [Test]
    public void ApplyResonance_ValidMaterial_IncrementsLevel() {
        var t  = Meta("tripleS", "ChaeYeon", "Special");
        var m  = Meta("tripleS", "ChaeYeon", "Special");
        var ci = new CardInstance { ResonanceLevel = 7 };
        var r  = CardEnhancement.ApplyResonance(ci, t, m);
        Assert.AreEqual(CardEnhancement.EnhanceResult.Success, r);
        Assert.AreEqual(8, ci.ResonanceLevel);
    }

    [Test]
    public void ApplyResonance_AtMaxLevel_FailsWithoutIncrement() {
        var t  = Meta("tripleS", "ChaeYeon", "Special");
        var m  = Meta("tripleS", "ChaeYeon", "Special");
        var ci = new CardInstance { ResonanceLevel = CardInstance.MAX_ENHANCEMENT_LEVEL };
        var r  = CardEnhancement.ApplyResonance(ci, t, m);
        Assert.AreEqual(CardEnhancement.EnhanceResult.AtMaxLevel, r);
        Assert.AreEqual(CardInstance.MAX_ENHANCEMENT_LEVEL, ci.ResonanceLevel);
    }

    // === Harmony/Resonance levels are independent ===
    [Test]
    public void HarmonyAndResonance_AreIndependent() {
        var t = Meta("tripleS", "ChaeYeon", "Special");
        var harmonyMat   = Meta("tripleS", "SeoYeon",  "Special");
        var resonanceMat = Meta("tripleS", "ChaeYeon", "Special");
        var ci = new CardInstance();
        CardEnhancement.ApplyHarmony(ci, t, harmonyMat);
        CardEnhancement.ApplyResonance(ci, t, resonanceMat);
        Assert.AreEqual(1, ci.HarmonyLevel);
        Assert.AreEqual(1, ci.ResonanceLevel);
    }
}
