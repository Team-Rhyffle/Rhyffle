using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour {
    [Header("References (인스펙터 와이어링)")]
    public Conductor       conductor;
    public NoteScreen      noteScreen;
    public JudgeProcessor  judgeProcessor;
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI comboText;
    public TMPro.TextMeshProUGUI judgementText;

    [Header("Note Prefabs (NoteCreator 흡수 — council 권장 6 루트 축소안)")]
    public GameObject basicNotePrefab;
    public GameObject slideNotePrefab;
    public GameObject flickNoteUpPrefab;
    public GameObject flickNoteRightPrefab;
    public GameObject flickNoteDownPrefab;
    public GameObject flickNoteLeftPrefab;
    public GameObject holdNotePrefab;
    public GameObject holdNoteBodyPrefab;

    [Header("Card System (Sprint 1.5+) — optional, scene-found fallback in Start")]
    public CardSystem cardSystem;
    public CardBoardUI cardBoardUI;   // Sprint 1.5.1: hover signal target

    [Header("Chart")]
    public string chartName = "dummy_test";

    [Header("Debug")]
    [Tooltip("ON 시 모든 노트를 판정선 도달 순간 자동 Perfect 처리 — 콤보/점수 검증용")]
    public bool autoPerfectDebug = false;   // changed: Sprint 1.5.2 — 실 입력 검증 default
    [Tooltip("Play 진입 후 곡 시작까지 대기 시간 (초). 첫 프레임 hiccup 흡수.")]
    public float startDelay = 2.0f;

    private ChartData chart;
    private List<Note> activeNotes = new List<Note>();
    private int normalIdx, holdIdx, slideIdx, flickIdx;
    private int totalNoteCount;
    private int score;
    private int combo;
    private int baseScore;
    private bool isPaused;
    private float startCountdown;
    private bool songStarted;
    private int _keyNoteTriggerCount;
    private LaneAnchor _hoveredLane;   // 현재 hover 중인 lane 추적 (전환 감지용)

    void Start() {
        // 채보 로드
        chart = ChartLoader.Load(chartName);
        totalNoteCount =
            chart.NormalNotes.Count +
            chart.HoldNotes.Count +    // 단순 카운트 (사이판정 노트별 분모는 Sprint 2)
            chart.SlideNotes.Count +
            chart.FlickNotes.Count;
        if (totalNoteCount > 0) {
            baseScore = Mathf.RoundToInt(1_000_000f / totalNoteCount);
        }
        // Fallback: locate CardSystem/CardBoardUI in scene if not inspector-wired.
        if (cardSystem == null)   cardSystem   = FindObjectOfType<CardSystem>();
        if (cardBoardUI == null)  cardBoardUI  = FindObjectOfType<CardBoardUI>();
        // Publish initial FieldChangedEvent so card subscribers see the starting field (may be empty in 1.5).
        if (cardSystem != null) {
            EventBus.Publish(new FieldChangedEvent { Field = cardSystem.GetField() });
        }

        // 시작 — startDelay 후 conductor.StartSong()
        startCountdown = startDelay;
        songStarted = false;
        Debug.Log($"[GameLoop] Loaded chart '{chartName}': {totalNoteCount} notes total, baseScore={baseScore}. Starting in {startDelay}s.");
    }

    void Update() {
        if (isPaused) return;
        // 시작 카운트다운 — Play 진입 hiccup 흡수 후 conductor 시작
        if (!songStarted) {
            startCountdown -= Time.deltaTime;
            if (startCountdown <= 0f) {
                conductor.StartSong();
                songStarted = true;
                Debug.Log($"[GameLoop] Conductor started. SongTime={conductor.SongTime:F3}, dspTime={AudioSettings.dspTime:F3}");
            }
            return;
        }
        SpawnDueNotes();
        DropActiveNotes();
        if (autoPerfectDebug) AutoPerfectJudge();
        else ProcessInput();
        CheckMissedNotes();
        UpdateHoverRaycast();   // 순수 visual — autoPerfectDebug 무관하게 항상 실행
    }

    // === Debug: 모든 노트가 판정선 도달 시점에 자동 Perfect 처리 ===
    private void AutoPerfectJudge() {
        float songTime = conductor.SongTime;
        for (int i = 0; i < activeNotes.Count; i++) {
            var note = activeNotes[i];
            if (note == null || note.IsJudged) continue;
            float noteTimeSec = Conductor.StepToTime(note.Position, conductor.bpm);
            // 노트가 판정선 도달(또는 통과) 직후 첫 프레임에 Perfect — visual ↔ judgment 일치
            if (songTime >= noteTimeSec) {
                Debug.Log($"[AutoJudge] note(line={note.Line}, pos={note.Position}) judged at songTime={songTime:F3}, noteTime={noteTimeSec:F3}, sprite.y={note.transform.position.y:F3}, judgeLineY={GameConfig.JUDGE_LINE_Y}");
                ApplyJudgment(note, JudgmentGrade.Perfect);
            }
        }
    }

    // === Spawn (Sprint 1 내부 메서드 — Week 2 NoteSpawner 분리) ===
    private void SpawnDueNotes() {
        float currentStep = conductor.CurrentStep;
        float spawnLead   = GameConfig.SPAWN_LEAD_STEPS;

        while (normalIdx < chart.NormalNotes.Count &&
               chart.NormalNotes[normalIdx].position - currentStep <= spawnLead) {
            var d = chart.NormalNotes[normalIdx++];
            SpawnNote(basicNotePrefab, NoteKind.Normal, d.position, d.line, d.length);
        }
        while (slideIdx < chart.SlideNotes.Count &&
               chart.SlideNotes[slideIdx].position - currentStep <= spawnLead) {
            var d = chart.SlideNotes[slideIdx++];
            SpawnNote(slideNotePrefab, NoteKind.Slide, d.position, d.line, d.length);
        }
        while (flickIdx < chart.FlickNotes.Count &&
               chart.FlickNotes[flickIdx].position - currentStep <= spawnLead) {
            var d = chart.FlickNotes[flickIdx++];
            var prefab = (FlickDirection)d.direction switch {
                FlickDirection.Up    => flickNoteUpPrefab,
                FlickDirection.Right => flickNoteRightPrefab,
                FlickDirection.Down  => flickNoteDownPrefab,
                FlickDirection.Left  => flickNoteLeftPrefab,
                _                    => flickNoteUpPrefab,
            };
            var note = SpawnNote(prefab, NoteKind.Flick, d.position, d.line, d.length);
            note.Direction = (FlickDirection)d.direction;
        }
        // HoldNote는 group 처리가 복잡 — Sprint 1 Week 2에 본격 (스파이크 단계엔 첫 keypoint만 spawn)
        while (holdIdx < chart.HoldNotes.Count &&
               chart.HoldNotes[holdIdx].position - currentStep <= spawnLead) {
            var d = chart.HoldNotes[holdIdx++];
            SpawnNote(holdNotePrefab, NoteKind.Hold, d.position, d.line, d.length);
        }
    }

    private Note SpawnNote(GameObject prefab, NoteKind kind, int position, int line, int length) {
        var go = Instantiate(prefab);  // Sprint 1: 풀링 미도입
        var note = go.GetComponent<Note>();
        if (note == null) note = go.AddComponent<Note>();
        note.Initialize(kind, position, line, length);
        activeNotes.Add(note);
        return note;
    }

    // === Drop (Sprint 1 내부 메서드 — Week 2 NoteDropper 분리) ===
    private void DropActiveNotes() {
        float songTime = conductor.SongTime;
        foreach (var note in activeNotes) {
            if (note != null && !note.IsJudged) {
                note.UpdatePosition(songTime, conductor.bpm, GameConfig.DEFAULT_SPEED);
            }
        }
    }

    // === Input (Sprint 1 내부 메서드 — Week 2 InputRouter 분리) ===
    private void ProcessInput() {
        // Touch + Editor mouse 통합. Unity Input System.
        var touches = UnityEngine.InputSystem.Touchscreen.current?.touches;
        if (touches != null) {
            foreach (var touch in touches) {
                if (touch.press.wasPressedThisFrame) {
                    HandlePress(touch.position.ReadValue());
                }
            }
        }
        // Editor 마우스 fallback
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) {
            HandlePress(mouse.position.ReadValue());
        }
    }

    private void HandlePress(Vector2 screenPos) {
        // 스크린 → 월드 raycast → LaneAnchor hit → lane index 추출
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f)) {
            var laneAnchor = hit.collider.GetComponent<LaneAnchor>();
            if (laneAnchor != null) {
                int lane = laneAnchor.laneIndex;
                if (cardBoardUI != null) cardBoardUI.OnLaneHover(lane);
                JudgeLanePress(lane);
            }
        }
    }

    // === Mouse hover highlight (Sprint 1.5.2 T4) ===
    // 매 프레임 마우스 위치 raycast → LaneAnchor 감지 → 전환 시에만 색 갱신.
    // laneIndex -1 을 OnLaneHover에 넘길 때 CardBoardUI 쪽 guard가 처리하므로 안전.
    private void UpdateHoverRaycast() {
        var mouse = UnityEngine.InputSystem.Mouse.current;
        if (mouse == null) {
            // 마우스 디바이스 없음 — hover 상태 초기화
            if (_hoveredLane != null) {
                _hoveredLane.SetHover(false);
                _hoveredLane = null;
                if (cardBoardUI != null) cardBoardUI.OnLaneHover(-1);   // -1 = clear (no-op in CardBoardUI guard)
            }
            return;
        }
        Vector2 screenPos = mouse.position.ReadValue();
        LaneAnchor newHover = null;
        if (Camera.main != null) {
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f)) {
                newHover = hit.collider.GetComponent<LaneAnchor>();
            }
        }
        if (newHover == _hoveredLane) return;   // 변경 없음 — early out
        // Transition: 이전 lane 복귀 → 새 lane highlight
        if (_hoveredLane != null) _hoveredLane.SetHover(false);
        if (newHover != null) {
            newHover.SetHover(true);
            if (cardBoardUI != null) cardBoardUI.OnLaneHover(newHover.laneIndex);
        }
        _hoveredLane = newHover;
    }

    private void JudgeLanePress(int lane) {
        var note = judgeProcessor.FindNearestNoteInLane(lane, activeNotes, out float distance);
        if (note == null) return;
        if (distance > (1f + GameConfig.MISS_K * GameConfig.DEFAULT_SPEED) * GameConfig.NOTE_HEIGHT) return;
        var grade = JudgeProcessor.ComputeGrade(distance, GameConfig.NOTE_HEIGHT, GameConfig.DEFAULT_SPEED);
        ApplyJudgment(note, grade);
    }

    // === Score + Combo (Sprint 1.5 EventBus 통합 — modifier 합산 + 콤보 보호 + 키노트 stub) ===
    private void ApplyJudgment(Note note, JudgmentGrade grade) {
        note.IsJudged = true;
        int prevCombo = combo;

        // Base score from JudgeProcessor (no cards yet)
        int baseDelta = Mathf.RoundToInt(baseScore * JudgeProcessor.GetScoreMultiplier(grade));

        // Publish NoteJudgedEvent — gives cards a chance to update internal state.
        // Modifiers list is provided but unused by the published event itself in current design
        // (cards expose modifiers via CardSystem.GetScoreMultipliers, not via the event payload).
        EventBus.Publish(new NoteJudgedEvent {
            Grade     = grade,
            Kind      = note.Kind,
            Line      = note.Line,
            Combo     = combo,                // pre-update value; ComboChangedEvent below carries the transition
            BaseScore = baseDelta,
            Modifiers = new System.Collections.Generic.List<ScoreModifier>(),
        });

        // Aggregate score multipliers from active card effects (left→right product).
        float productMul = 1f;
        if (cardSystem != null) {
            // Sprint 1.5.1: card modifier 영역 한정 — note.Line의 lane이 속한 카드 슬롯의 effect만 적용
            int cardIdx = GameConfig.LaneToCardIndex(note.Line);
            var mods = cardSystem.GetScoreMultipliersForSlot(cardIdx);
            foreach (var m in mods) productMul *= m.Multiplier;
        }
        int finalDelta = Mathf.RoundToInt(baseDelta * productMul);
        score += finalDelta;

        // Combo update with protection on Miss
        // Sprint 1.5.1 design: combo 보호는 전체 영역 적용 (어느 카드의 def도 트리거 가능). 영역 한정은 spec 미정 — Sprint 2.
        if (grade == JudgmentGrade.Miss) {
            bool protected_ = cardSystem != null && cardSystem.TryConsumeComboProtection();
            if (!protected_) combo = 0;
            // else: combo unchanged
        } else {
            combo++;
        }
        EventBus.Publish(new ComboChangedEvent { Combo = combo, Previous = prevCombo });

        // Key note stub: every KEY_NOTE_COMBO_INTERVAL combo step (only when combo just crossed a boundary)
        if (combo > 0 && combo % GameConfig.KEY_NOTE_COMBO_INTERVAL == 0 && combo != prevCombo) {
            _keyNoteTriggerCount++;
            EventBus.Publish(new KeyNoteProcessedEvent {
                Combo = combo,
                TriggerCount = _keyNoteTriggerCount,
            });
        }

        UpdateHUD(grade);
        Debug.Log($"[Judge] {grade} note(kind={note.Kind}, line={note.Line}) base={baseDelta} ×{productMul:0.00} = {finalDelta} | score={score} combo={combo}");
    }

    private void UpdateHUD(JudgmentGrade grade) {
        if (scoreText != null) scoreText.text = score.ToString("N0");
        if (comboText != null) comboText.text = combo > 0 ? combo.ToString() : "";
        if (judgementText != null) judgementText.text = grade.ToString();
    }

    // === Miss check (시간 지난 노트) ===
    private void CheckMissedNotes() {
        float songTime = conductor.SongTime;
        float missTimeThreshold = (1f + GameConfig.MISS_K * GameConfig.DEFAULT_SPEED) * GameConfig.NOTE_HEIGHT
                                / (GameConfig.SPAWN_Y - GameConfig.JUDGE_LINE_Y)
                                * Conductor.StepToTime(GameConfig.SPAWN_LEAD_STEPS, conductor.bpm);
        for (int i = activeNotes.Count - 1; i >= 0; i--) {
            var note = activeNotes[i];
            if (note == null) { activeNotes.RemoveAt(i); continue; }
            float noteTimeSec = Conductor.StepToTime(note.Position, conductor.bpm);
            if (!note.IsJudged && songTime - noteTimeSec > missTimeThreshold) {
                ApplyJudgment(note, JudgmentGrade.Miss);
            }
            // 화면 밖 노트 destroy
            if (note.IsJudged && songTime - noteTimeSec > 1.0f) {
                activeNotes.RemoveAt(i);
                Destroy(note.gameObject);  // Sprint 1: 풀링 미도입
            }
        }
    }

    // === Pause / Resume ===
    public void Pause() {
        isPaused = true;
        conductor.Pause();
        if (noteScreen != null) noteScreen.SetBarsActive(false);
    }

    public void Resume() {
        // TODO: Pause sprite 3-2-1 카운트다운 (Week 2 후반)
        isPaused = false;
        conductor.Resume();
        if (noteScreen != null) noteScreen.SetBarsActive(true);
    }
}
