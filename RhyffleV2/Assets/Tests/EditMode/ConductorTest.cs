using NUnit.Framework;

public class ConductorTest {
    [Test]
    public void StepToTime_AtBPM120_OneQuarterIsHalfSecond() {
        // BPM 120 = 0.5초/박자. 1/4박자 = 48 step → 0.5s.
        float seconds = Conductor.StepToTime(48, 120f);
        Assert.AreEqual(0.5f, seconds, 0.0001f);
    }

    [Test]
    public void StepToTime_AtBPM120_OneSixtyFourthIsCorrect() {
        // BPM 120, 1/64 = 3 step → 0.5 / 16 = 0.03125s.
        float seconds = Conductor.StepToTime(3, 120f);
        Assert.AreEqual(0.03125f, seconds, 0.0001f);
    }

    [Test]
    public void TimeToStep_IsInverseOfStepToTime() {
        const float bpm = 120f;
        for (int step = 0; step < 192; step += 3) {
            float seconds = Conductor.StepToTime(step, bpm);
            float backStep = Conductor.TimeToStep(seconds, bpm);
            Assert.AreEqual(step, backStep, 0.001f);
        }
    }
}
