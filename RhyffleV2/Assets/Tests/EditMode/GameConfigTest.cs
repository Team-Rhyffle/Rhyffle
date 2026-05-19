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
        Assert.AreEqual(120f, GameConfig.DEFAULT_BPM);
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

    [Test]
    public void BarX_Formula_AtBoundaries() {
        // 25 marker 중심정렬: Bar_1=-12, Bar_13=0, Bar_25=+12
        Assert.AreEqual(-12f, GameConfig.BarX(1));
        Assert.AreEqual(0f,   GameConfig.BarX(13));
        Assert.AreEqual(12f,  GameConfig.BarX(25));
    }

    [Test]
    public void LaneX_Formula_AtBoundaries() {
        // 24 lane center: lane 0=-11.5, lane 12=0.5, lane 23=+11.5
        Assert.AreEqual(-11.5f, GameConfig.LaneX(0));
        Assert.AreEqual(0.5f,   GameConfig.LaneX(12));
        Assert.AreEqual(11.5f,  GameConfig.LaneX(23));
    }

    [Test]
    public void BarCount_And_LaneCount_AreCoherent() {
        Assert.AreEqual(25, GameConfig.BAR_COUNT);
        Assert.AreEqual(24, GameConfig.LANE_COUNT);
        Assert.AreEqual(GameConfig.BAR_COUNT, GameConfig.LANE_COUNT + 1);
    }

    [Test]
    public void FlickDirection_Ordinals_AreClockwise() {
        // 시계방향: 12시(Up) → 3시(Right) → 6시(Down) → 9시(Left)
        Assert.AreEqual(0, (int)FlickDirection.Up);
        Assert.AreEqual(1, (int)FlickDirection.Right);
        Assert.AreEqual(2, (int)FlickDirection.Down);
        Assert.AreEqual(3, (int)FlickDirection.Left);
    }

    [Test]
    public void CARDS_IN_FIELD_TimesLANES_PER_CARD_EqualsLaneCount() {
        // 24 lane / 8 card / 3 lane-per-card invariant
        Assert.AreEqual(8, GameConfig.CARDS_IN_FIELD);
        Assert.AreEqual(3, GameConfig.LANES_PER_CARD);
        Assert.AreEqual(GameConfig.LANE_COUNT, GameConfig.CARDS_IN_FIELD * GameConfig.LANES_PER_CARD);
    }

    [Test]
    public void LaneToCardIndex_MapsCorrectly() {
        // lane 0~2 → card 0, lane 3~5 → card 1, ..., lane 21~23 → card 7
        Assert.AreEqual(0, GameConfig.LaneToCardIndex(0));
        Assert.AreEqual(0, GameConfig.LaneToCardIndex(1));
        Assert.AreEqual(0, GameConfig.LaneToCardIndex(2));
        Assert.AreEqual(1, GameConfig.LaneToCardIndex(3));
        Assert.AreEqual(2, GameConfig.LaneToCardIndex(6));
        Assert.AreEqual(7, GameConfig.LaneToCardIndex(21));
        Assert.AreEqual(7, GameConfig.LaneToCardIndex(23));
    }
}
