using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Conductor : MonoBehaviour {
    public float bpm    = GameConfig.DEFAULT_BPM;
    public float offset = GameConfig.DEFAULT_OFFSET;

    private AudioSource audioSource;
    private double startDsp;            // 곡 시작 dspTime
    private double pauseStartDsp;       // pause 시점 dspTime
    private double pauseOffsetAccum;    // 누적 pause 시간
    private bool   isPaused;
    private bool   isStarted;

    /// <summary>
    /// 현재 곡 시간 (초). Pause 시간 제외. 곡 시작 전엔 음수.
    /// </summary>
    public float SongTime {
        get {
            if (!isStarted) return -1000f;
            if (isPaused) {
                return (float)(pauseStartDsp - startDsp - pauseOffsetAccum) - offset;
            }
            return (float)(AudioSettings.dspTime - startDsp - pauseOffsetAccum) - offset;
        }
    }

    /// <summary>
    /// 현재 곡 step (1/4박자 = 48 step).
    /// </summary>
    public float CurrentStep => TimeToStep(SongTime, bpm);

    public bool IsPaused => isPaused;

    void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// 곡 시작 — PlayScheduled로 dspTime과 첫 샘플을 정렬 (Council Scenario A 차단).
    /// </summary>
    public void StartSong() {
        // Android 오디오 버퍼 지연(50~150ms) + Editor Play 진입 hiccup 대비 0.3s 여유.
        double scheduledDsp = AudioSettings.dspTime + 0.3;
        if (audioSource.clip != null) {
            audioSource.PlayScheduled(scheduledDsp);
        }
        startDsp        = scheduledDsp;
        pauseOffsetAccum = 0.0;
        isPaused        = false;
        isStarted       = true;
    }

    /// <summary>
    /// 일시정지 — dspTime 기록 (dspTime 자체는 멈출 수 없으므로 offset 누적 패턴).
    /// </summary>
    public void Pause() {
        if (!isStarted || isPaused) return;
        if (audioSource.isPlaying) audioSource.Pause();
        pauseStartDsp = AudioSettings.dspTime;
        isPaused      = true;
    }

    /// <summary>
    /// 재개 — pause 시간을 offset에 누적.
    /// </summary>
    public void Resume() {
        if (!isStarted || !isPaused) return;
        pauseOffsetAccum += AudioSettings.dspTime - pauseStartDsp;
        if (audioSource.clip != null) audioSource.UnPause();
        isPaused = false;
    }

    // === Pure helper (테스트 가능) ===

    /// <summary>
    /// chart step → seconds (BPM 기준). 1/4박자 = 48 step.
    /// </summary>
    public static float StepToTime(int step, float bpm) {
        // 1박자 = 60/bpm 초, 1 step = 60/bpm/48 초
        return (step * 60f) / (bpm * GameConfig.STEPS_PER_QUARTER);
    }

    /// <summary>
    /// seconds → chart step (소수점 포함).
    /// </summary>
    public static float TimeToStep(float seconds, float bpm) {
        return (seconds * bpm * GameConfig.STEPS_PER_QUARTER) / 60f;
    }
}
