using NUnit.Framework;

public class GameConfigTest {
    [Test]
    public void Constants_HaveExpectedValues() {
        Assert.AreEqual(24, GameConfig.LANE_COUNT);
        Assert.AreEqual(1.0f, GameConfig.LANE_WIDTH);
        Assert.AreEqual(1.0f, GameConfig.NOTE_HEIGHT);
        Assert.AreEqual(-2.5f, GameConfig.JUDGE_LINE_Y);
        Assert.AreEqual(7.75f, GameConfig.SPAWN_Y);
        Assert.AreEqual(48, GameConfig.STEPS_PER_QUARTER);
        Assert.AreEqual(120, GameConfig.DEFAULT_BPM);
        Assert.AreEqual(1.0f, GameConfig.DEFAULT_SPEED);
        Assert.AreEqual(10.0f, GameConfig.CAMERA_ORTHO_SIZE);
    }

    [Test]
    public void JudgmentMultipliers_AreCorrect() {
        Assert.AreEqual(1.2f, GameConfig.PERFECT_MULT);
        Assert.AreEqual(1.0f, GameConfig.GREAT_MULT);
        Assert.AreEqual(0.7f, GameConfig.GOOD_MULT);
        Assert.AreEqual(0.0f, GameConfig.MISS_MULT);
    }

    [Test]
    public void JudgmentThresholds_K_AreCorrect() {
        Assert.AreEqual(0.2f, GameConfig.PERFECT_K);
        Assert.AreEqual(0.4f, GameConfig.GREAT_K);
        Assert.AreEqual(0.7f, GameConfig.GOOD_K);
        Assert.AreEqual(1.0f, GameConfig.MISS_K);
    }
}
