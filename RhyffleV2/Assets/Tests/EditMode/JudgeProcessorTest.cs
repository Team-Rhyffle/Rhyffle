using NUnit.Framework;

public class JudgeProcessorTest {
    const float H = 1.0f;
    const float R = 1.0f;

    [Test]
    public void ComputeGrade_DistanceWithinPerfect_ReturnsPerfect() {
        // (1 + 0.2·r)·h = 1.2
        Assert.AreEqual(JudgmentGrade.Perfect, JudgeProcessor.ComputeGrade(0.0f, H, R));
        Assert.AreEqual(JudgmentGrade.Perfect, JudgeProcessor.ComputeGrade(1.0f, H, R));
        Assert.AreEqual(JudgmentGrade.Perfect, JudgeProcessor.ComputeGrade(1.19f, H, R));
    }

    [Test]
    public void ComputeGrade_DistanceWithinGreat_ReturnsGreat() {
        // Perfect 1.2 < d ≤ Great 1.4
        Assert.AreEqual(JudgmentGrade.Great, JudgeProcessor.ComputeGrade(1.25f, H, R));
        Assert.AreEqual(JudgmentGrade.Great, JudgeProcessor.ComputeGrade(1.4f, H, R));
    }

    [Test]
    public void ComputeGrade_DistanceWithinGood_ReturnsGood() {
        // Great 1.4 < d ≤ Good 1.7
        Assert.AreEqual(JudgmentGrade.Good, JudgeProcessor.ComputeGrade(1.5f, H, R));
        Assert.AreEqual(JudgmentGrade.Good, JudgeProcessor.ComputeGrade(1.7f, H, R));
    }

    [Test]
    public void ComputeGrade_DistanceWithinMiss_ReturnsMiss() {
        // Good 1.7 < d ≤ Miss 2.0
        Assert.AreEqual(JudgmentGrade.Miss, JudgeProcessor.ComputeGrade(1.8f, H, R));
        Assert.AreEqual(JudgmentGrade.Miss, JudgeProcessor.ComputeGrade(2.0f, H, R));
    }

    [Test]
    public void ComputeGrade_DistanceBeyondMiss_ReturnsMiss() {
        Assert.AreEqual(JudgmentGrade.Miss, JudgeProcessor.ComputeGrade(2.5f, H, R));
        Assert.AreEqual(JudgmentGrade.Miss, JudgeProcessor.ComputeGrade(100f, H, R));
    }

    [Test]
    public void GetScoreMultiplier_ReturnsCorrectValues() {
        Assert.AreEqual(1.2f, JudgeProcessor.GetScoreMultiplier(JudgmentGrade.Perfect));
        Assert.AreEqual(1.0f, JudgeProcessor.GetScoreMultiplier(JudgmentGrade.Great));
        Assert.AreEqual(0.7f, JudgeProcessor.GetScoreMultiplier(JudgmentGrade.Good));
        Assert.AreEqual(0.0f, JudgeProcessor.GetScoreMultiplier(JudgmentGrade.Miss));
    }
}
