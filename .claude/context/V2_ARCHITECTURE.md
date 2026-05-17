# Rhyffle v2 — Sprint 1 아키텍처 청사진

> Sprint 260514 (Plain Mode 플레이 화면 1차 구현) 기준.
> v1 God Class (GamePlayer 570줄) → v2 책임 분리. 이벤트 기반 통신, 풀링, 단일 시간 SoT.
> 카드/시너지/족보 제외. 향후 sprint에서 확장 시 module 추가.

---

## 1. 컴포넌트 다이어그램

```
                          ┌───────────────┐
                          │  GameConfig   │ (상수 + 곡 메타)
                          └───────┬───────┘
                                  │ 참조
                                  ▼
┌─────────────┐  ┌──────────┐  ┌──────────────┐
│ ChartLoader │─▶│ Conductor│  │  InputRouter │
└─────────────┘  └─────┬────┘  └──────┬───────┘
                       │ SongTime     │ LaneInputEvent
                       ▼              │
                ┌─────────────┐       │
                │NoteSpawner  │       │
                └──────┬──────┘       │
                       │ Spawn        │
                       ▼              │
                ┌─────────────┐       │
                │ Note Pool   │       │
                │ (active)    │       │
                └──────┬──────┘       │
                       │ Active List  │
                       ▼              │
                ┌─────────────┐       │
                │NoteDropper  │       │
                └──────┬──────┘       │
                       │              │
                       ▼              ▼
                ┌──────────────────────────┐
                │     JudgeProcessor       │
                │  (입력 × 활성 노트)       │
                └──────────┬───────────────┘
                           │ JudgmentEvent
              ┌────────────┼─────────────┐
              ▼            ▼             ▼
        ┌──────────┐ ┌──────────┐ ┌─────────────┐
        │  Score   │ │  Combo   │ │HUDController│
        └────┬─────┘ └────┬─────┘ └─────────────┘
             │ScoreChanged│ComboChanged
             └─────┬──────┘
                   ▼ events
              ┌─────────────┐
              │HUDController│
              └─────────────┘
```

## 2. 단일 시간 SoT (Source of Truth) — Conductor

리듬게임의 모든 비극은 **시간 단위가 통일 안 되면** 시작됨. v1의 `bpm=192`, `chartToolOffset=6.4` 매직 같은 거.

### Conductor 책임
- 곡 시간 (`SongTime`, seconds, 음수=카운트다운)
- 채보 step 단위 (`StepTime` — chart position 단위와 1:1)
- BPM, 오프셋, playerSpeed (배속)
- 변환 메서드:
  - `StepToTime(int chartPosition) → float seconds`
  - `TimeToStep(float seconds) → float step`
  - `IsPaused`, `Pause()`, `Resume()`

### 시간 식 (제안)
- step 단위: 채보툴 spec 그대로 (3=1/64, 48=1/4)
- 1 step = `60 / BPM / 48` 초 (1/4박자 = 48 step 기준)
- `SongTime = AudioSource.time + Offset`
- `CurrentStep = SongTime × BPM × 48 / 60`

→ 채보의 노트 `position` 과 동일 단위로 직접 비교 가능. 변환 한 곳에서만.

---

## 3. 컴포넌트별 책임 + v1 매핑

### 3.1 `GameConfig` (정적 상수 + 곡 메타)
**책임**: 매직 넘버 단일 모음.
```csharp
public static class GameConfig {
    public const int SLOT_COUNT = 8;
    public const int LANE_COUNT = 24;          // 8 × 3
    public const int STEPS_PER_QUARTER = 48;   // 1/4박자 = 48 step
    public const int SPAWN_LEAD_STEPS = 64;    // 노트 도착 전 step (1박자 ≈ 64*4/speed)
    
    // 판정 윈도우 계수 (h 단위)
    public const float PERFECT_K = 0.2f;
    public const float GREAT_K = 0.4f;
    public const float GOOD_K = 0.7f;
    public const float MISS_K = 1.0f;
    
    // 점수 배율
    public const float PERFECT_MULT = 1.2f;
    public const float GREAT_MULT = 1.0f;
    public const float GOOD_MULT = 0.7f;
    public const float MISS_MULT = 0.0f;
    
    // 노트 시각 상수 (Hi-Fi 확정 후 갱신. 2026-05-17 v1 인스펙션 실측 반영)
    public const float NOTE_HEIGHT = 1.0f;     // h, world units (placeholder)
    public const float JUDGE_LINE_Y = -2.5f;   // world y of 판정선 (v1 Bar y 실측 — 기존 -2.25 정정)
    public const float SPAWN_Y = 7.75f;        // world y of 출발선 (v1 값)
    public const float LANE_WIDTH = 1.0f;      // lane 간격 (v1 21 lane Bar 간격 실측. v2 24 lane은 1.0 그대로 또는 화면 fit 0.875 축소 중 선택)
    
    // 카메라 셋업 (v1 동일 채택)
    public const float CAMERA_ORTHO_SIZE = 10.0f;
    // Main Camera 위치 (0, 1.5, -10), background RGB(49, 77, 121)
    
    // Canvas 셋업 (v1 동일 채택)
    public const int CANVAS_REF_WIDTH = 956;
    public const int CANVAS_REF_HEIGHT = 440;
    // ScreenSpaceCamera mode, planeDistance 100, ScreenMatchMode = Expand
}

public class SongMeta {
    public string Name;
    public string ChartPath;
    public string AudioPath;
    public float BPM;
    public float Offset;
}
```
**v1 출처**: 곳곳에 박힌 매직 넘버 흡수. `Define.cs` 의 enum은 별도 유지.

### 3.2 `Conductor`
**책임**: 시간 단일 SoT, BPM/오프셋 변환.
**의존**: `GameConfig`, `AudioSource`
**v1 매핑**:
- `GamePlayer.currentTime/bpm/chartToolOffset` → 이곳으로
- `AudioTest.Play()/Pause()/Resume()` 로직 흡수 (또는 obs)
**v1 인스펙션 (2026-05-17)**:
- v1 AudioSource 셋업: `volume=0.3, pitch=1, spatialBlend=0 (2D), playOnAwake=false, loop=false`
- v1 시간 소스 = `AudioSource.time` + `AudioTest.timer`. v2는 council 결정대로 **`AudioSettings.dspTime`** 으로 (jitter 회피)
- v1 음원 자산 경로 패턴: `Resources/Sounds/Songs/{nn}_{artist}_{title}.wav`
- Sprint 1엔 AudioSource clip 없이 dspTime만으로 가상 시계 (오디오 미통합 OK — 음원/곡 메타 spec 후순위)

### 3.3 `ChartLoader`
**책임**: JSON 파일 → `ChartData` 객체.
**의존**: 없음 (Pure)
**v1 매핑**:
- `JsonManager.LoadJson() + ReturnJson()` → `ChartLoader.Load(path) → ChartData`
- `NoteType.cs` (NoteJson 컨테이너) → `ChartData` 로 rename (혼란 방지)

### 3.4 `NoteSpawner`
**책임**: Conductor 시간 보고 timing 도래한 노트 인스턴스화.
**의존**: `Conductor`, `NotePool`, `ChartData`
**알고리즘**:
- 4종 인덱스 (basicCur 등) 유지
- `while (currentStep + SPAWN_LEAD_STEPS >= chart.NormalNotes[basicCur].position) → Pool.Get → Set → push to active list → basicCur++`
- HoldNote는 count 그룹 단위로 묶어 `HoldNoteBody` 1개로 spawn
**v1 매핑**:
- `GamePlayer.GameSystem()` 의 `[SPAWN]` region → 이 클래스
- `NoteCreator.Create*` → `NotePool.Get<T>` 으로 대체

### 3.5 `NotePool`
**책임**: Note prefab 풀링 (Instantiate/Destroy 회피).
**v1 매핑**:
- `Managers.Pool` 활용 (이미 존재) 또는 신규
- `NoteCreator.Instantiate` → `Pool.Get<BasicNote>` 등
**v1 인스펙션 (2026-05-17)** — NoteCreator 인스펙터 ref 5개 (모두 `Assets/Resources/Prefabs/Note/`):
- `BasicNote.prefab` (탭)
- `SlideNote.prefab` (슬라이드)
- `FlickNote.prefab` (플릭, 방향)
- `HoldNote.prefab` (홀드 사이판정 노트 단위)
- `HoldNoteBody.prefab` (홀드 폴리라인 비주얼)
→ v2 salvage 시 `.meta` 동반 복사 + 동일 경로로 v2 Resources에 배치

### 3.6 `NoteDropper`
**책임**: 매 프레임 모든 활성 노트의 위치 갱신.
**의존**: `Conductor` (시간), active note list
**v1 매핑**:
- `GamePlayer.GameSystem()` 의 `[DROP]` region
- `Note.Drop(speed)` 호출 그대로

### 3.7 `Note` (런타임 객체)
**책임**: 자신의 위치/시각 표현. 판정은 외부에서.
**구조**:
- `int Position` (chart step)
- `int Line` (0~23, leftmost lane)
- `int Length` (가로 폭)
- `NoteKind Kind` (Normal/Hold/Slide/Flick — enum)
- `FlickDirection? Direction` (Flick 한정, enum 0=Up/1=Right/2=Down/3=Left)
- `void UpdatePosition(float songTime)` — Conductor 보고 자기 위치 계산
- `Vector2 GetCenter()` — 판정용 중심 좌표

**v1과의 차이**:
- BasicNote/SlideNote/FlickNote/HoldNote 단순 래퍼 → 통합 또는 슬림화
- `ReadJudge` 메서드 제거 → JudgeProcessor가 외부에서 거리 계산
- 풀링 호환을 위해 `Reset()` 메서드 필요

### 3.8 `HoldNoteBody` (특수)
**책임**: 사이 판정 노트들 묶음, 폴리라인 렌더링.
**구조**:
- `List<Note> Keypoints` (사이 판정 노트들, position 정렬됨)
- `int CurrentKeypointIndex` (판정 진행 상태)
- 폴리라인 LineRenderer (widthCurve 한 번 생성, 키만 갱신)
**v1 매핑**:
- `HoldNoteBody.cs` 구조 차용 + DrawLine GC 최적화

### 3.9 `InputRouter`
**책임**: 터치/마우스 입력 → 이벤트 통합.
**이벤트**:
```csharp
public event Action<int, float> OnLanePress;     // lane, songTime
public event Action<int, float> OnLaneRelease;   // lane, songTime
public event Action<int, float> OnLaneHold;      // lane, songTime (매 프레임)
public event Action<int, FlickDirection, float> OnLaneFlick; // lane, dir, songTime
```
**v1 매핑**:
- `NoteScreen.cs` 터치 추적 + `Bar.cs` 마우스 입력 통합
- 플릭 4방향 각도 분류 (-45~45 = Up, 45~135 = Right, 135~-135 wrapping = Down, -135~-45 = Left)
- GamePlayer 직접 접근 제거 → 이벤트 publish
**v1 인스펙션 (2026-05-17)**:
- v1은 **Unity Input System** 패키지 사용 (legacy `Input.*` 아님). EventSystem에 `InputSystemUIInputModule` 부착됨. v2 동일 채택
- v1 Bar 콜라이더 = **3D `BoxCollider`** (BoxCollider2D 아님). ortho camera + 3D Physics 조합. v2 동일 시 raycast 코드 동일
- v1 GamePlayer 입력 상태 = bool[21] × 6 + int[21] × 3 (press/slide/intouch/endtouch/flickUp/flickDown/touchStart/touchEnd/touchCon). v2는 이 평행 배열 제거하고 이벤트 publish로 대체
- 플릭 영역 마커: v1 NoteScreen 자식에 `FlickUpZone` / `FlickDownZone` 빈 GameObject 존재 (좌우 플릭 추가 전 잔재 — v2에선 4방향이라 zone 4개 또는 각도 기반으로 폐기)

### 3.10 `JudgeProcessor`
**책임**: 입력 이벤트 + 활성 노트 → 거리 계산 → JudgmentResult.
**의존**: `Conductor`, `NoteSpawner` (활성 노트 query), `GameConfig`
**알고리즘**:
1. `InputRouter.OnLanePress` 구독 → 해당 lane의 가장 가까운 NormalNote/HoldNote 첫 키포인트
2. 거리 = |노트 위치 - 판정선| (또는 `|songTime - StepToTime(position)| × dropSpeed`)
3. `(1 + k·r) × h` 임계로 등급 분류
4. `JudgmentEvent` 발행

**이벤트**:
```csharp
public event Action<JudgmentResult> OnJudgment;

public struct JudgmentResult {
    public JudgmentGrade Grade;  // Perfect/Great/Good/Miss
    public int NoteIndex;
    public int Lane;
    public bool IsFirstNote;
}
```

**v1 매핑**:
- `GamePlayer.GameSystem()` 의 `[JUDGE]` region (250줄) → 이 클래스
- `Note.ReadJudge` (시간 기반) → 폐기 + 거리 기반 신규
- `HitEvent` 클래스 → `JudgmentResult` struct로 슬림화 (cardEffect[] 제거)

### 3.11 `ScoreSystem`
**책임**: 판정 → 점수 누적.
**의존**: `JudgeProcessor.OnJudgment` 구독
**상태**: `int TotalScore`
**이벤트**: `event Action<int> OnScoreChanged`
**v1 매핑**:
- `Managers.Content.ScoreManager` → 그대로 사용 (이미 깔끔. Plain Mode 분기 있음)
- `GameScoreInfo` 의 UI 부분 → HUDController로 이동

### 3.12 `ComboCounter` (v2 신규)
**책임**: 콤보 카운트.
**구독**: `JudgeProcessor.OnJudgment`
**로직**:
- Perfect/Great/Good → `combo++`
- Miss → `combo = 0`
**이벤트**: `event Action<int> OnComboChanged`

### 3.13 `HUDController`
**책임**: 점수/콤보/판정 텍스트 UI 갱신.
**구독**: `ScoreSystem.OnScoreChanged`, `ComboCounter.OnComboChanged`, `JudgeProcessor.OnJudgment`
**v1 매핑**:
- `GameScoreInfo.text.text = ...` + `judgementText.JudgeText(...)` → 이 클래스로
**v1 인스펙션 (2026-05-17)** — Canvas 자식 UI 실측:
- **Score**: anchor (0, 0.5) — 좌측중앙, anchoredPos (25, 0), sizeDelta (100, 0), TMP
- **JudgementText**: anchor stretch (0,0)–(1,1), anchoredPos (0, -50), margin 400px, fontSize 60 bold, center align, TMP `LiberationSans SDF`
- **Combo (v1)**: anchor (1, 0.5) 우측중앙 + 내부 자식 텍스트 (0.5, 1) 상단중앙. v2는 **중앙 상단으로 의식적 변경** (2026-05-17 사용자 결정)
- **GameUIManager.countSprites[3]** = `Resources/Art/UI/Pause/{1,2,3}.png` — Pause 카운트다운 3-2-1 sprite. v2도 동일 salvage
- Canvas mode: ScreenSpaceCamera, planeDistance 100, ref res 956×440, ScreenMatchMode = Expand

### 3.14 `GameLoop` (Orchestrator, 150줄 이하 목표)
**책임**: 컴포넌트 초기화 + 실행 순서 + 일시정지.
**Update 순서** (또는 UniTask):
1. `Conductor.Tick(deltaTime)` (시간 진행)
2. `NoteSpawner.Update()` (timing 보고 spawn)
3. `NoteDropper.Update()` (모든 노트 위치 갱신)
4. `InputRouter.Update()` (터치 폴링 → 이벤트)
5. `JudgeProcessor.Update()` (Miss check — 시간 지난 노트)

→ InputRouter 이벤트는 자동으로 JudgeProcessor 트리거
→ Judgment 이벤트는 자동으로 Score/Combo/HUD 트리거

**v1 매핑**:
- `GamePlayer.GameSystem()` 의 골격만 — 200줄 → 80줄 목표

---

## 4. 이벤트 흐름 (Sequence)

```
[프레임 N]
Conductor.Tick(dt)
  ↓ SongTime 갱신
NoteSpawner.Update()
  ↓ timing 도달한 Note 인스턴스 (from Pool)
NoteDropper.Update()
  ↓ 모든 활성 Note의 UpdatePosition()
InputRouter.Update()
  ↓ Touch.phase 분석
  ↓ 각 lane별 OnLanePress / OnLaneFlick 등 publish
JudgeProcessor.OnLanePressHandler(lane, time)
  ↓ 가장 가까운 active note 찾기
  ↓ 거리 계산 → Grade 분류
  ↓ JudgmentResult publish
ScoreSystem.OnJudgmentHandler(result)
  ↓ ApplyNoteScore
  ↓ OnScoreChanged publish
ComboCounter.OnJudgmentHandler(result)
  ↓ combo 갱신
  ↓ OnComboChanged publish
HUDController.OnScoreChanged / OnComboChanged / OnJudgment
  ↓ UI 갱신
JudgeProcessor.Update()
  ↓ 시간 지난 노트 Miss 처리 → publish
```

---

## 5. 데이터 구조

### ChartData (DTO, 디시리얼라이즈 대상)
```csharp
[Serializable]
public class ChartData {
    public NoteData[] NormalNotes;
    public HoldData[] HoldNotes;
    public NoteData[] SlideNotes;
    public FlickData[] FlickNotes;
}

[Serializable] public struct NoteData { 
    public int position, line, length; 
}
[Serializable] public struct HoldData { 
    public int position, line, count, length;  // noteType 제거됨
}
[Serializable] public struct FlickData { 
    public int position, line, length, direction;  // direction 0~3 (시계방향)
}
```

### Runtime Note
```csharp
public enum NoteKind { Normal, Hold, Slide, Flick }
public enum FlickDirection { Up = 0, Right = 1, Down = 2, Left = 3 }
public enum JudgmentGrade { Perfect, Great, Good, Miss }

public class Note : MonoBehaviour, IPoolable {
    public NoteKind Kind;
    public int Position;     // chart step
    public int Line;
    public int Length;
    public FlickDirection? Direction;  // Flick 한정
    
    public void Reset();
    public void UpdatePosition(float songTime, float speed);
    public bool ContainsLane(int lane) => lane >= Line && lane < Line + Length;
}
```

---

## 6. v1 → v2 컴포넌트 매핑 표

| v1 파일 | v2 컴포넌트 | 비고 |
|---|---|---|
| `Define.cs` | enum 분리 → 각 도메인별 | JudgmentType 결과/상태 분리 |
| `Utils/NoteType.cs` | `ChartData.cs` | noteType 필드 제거, rename |
| `Managers/Content/JsonManager.cs` | `ChartLoader.cs` | 2단계 호출 → 단일 |
| `Contents/Note/Note/*.cs` (Note/Basic/Slide/Flick/Hold) | `Note.cs` 통합 | 단순 래퍼 제거 |
| `Contents/Note/Note/HoldNoteBody.cs` | `HoldNoteBody.cs` | DrawLine GC 최적화 |
| `Contents/NoteCreator.cs` | `NotePool.cs` + `NoteSpawner.cs` | 풀링 도입 |
| `Contents/Note/NoteScreen.cs` | `InputRouter.cs` (터치) | 이벤트 publish |
| `Contents/Note/Bar.cs` | `InputRouter.cs` (마우스, Editor) | 통합 |
| `Utils/HitEvent.cs` | `JudgmentResult.cs` (struct) | cardEffect 제거, 슬림 |
| `Managers/Content/ScoreManager.cs` | `ScoreSystem.cs` | 거의 그대로. Plain Mode 분기 유지 |
| `Contents/GameScoreInfo.cs` | `HUDController.cs` + `ScoreSystem` | 책임 분리. handRankScore는 Sprint 2+ |
| `AudioTest.cs` | `Conductor.cs` (포함) | timer 매직 → Offset spec |
| `Contents/GamePlayer.cs` (570줄) | `GameLoop.cs` (얇음) + 위 컴포넌트들 | 분할 |
| `Managers/Managers.cs` | (유지) | Sprint 1 Card/Deck/Effect/Hand 안 씀, 빈 stub 또는 lazy init |
| 신규 | `GameConfig.cs` | 매직 넘버 흡수 |
| 신규 | `Conductor.cs` | 시간 SoT |
| 신규 | `ComboCounter.cs` | Sprint 1 spec 신규 |

---

## 7. 풀링 전략

`Managers.Core.PoolManager` (v1 존재) 활용 또는 단순 `Stack<Note>` 풀.

- 4종 prefab × 각 풀
- `Spawn`: `pool.Pop()` → `Reset(noteData)` → 활성 리스트
- `Despawn`: 활성 리스트 제거 → `Reset()` → `pool.Push()`
- 풀 비면 Instantiate (warm-up 시 미리 N개 채워두면 GC 0)

Sprint 1엔 단순 Stack 풀로 시작, 빠른 곡 테스트 시 GC 측정 후 최적화.

---

## 8. 시간 모델 (Update vs UniTask)

**선택**: 기본은 **`Update()`** + `bool isPaused` 플래그.
- 단순함, 표준
- UniTask는 일시정지 코드를 `await WaitUntil` 한 줄로 만들어주지만 의존성 추가됨
- 일시정지 처리:
  ```csharp
  void Update() {
      if (isPaused) return;
      conductor.Tick(Time.deltaTime);
      noteSpawner.Process();
      noteDropper.Process();
      inputRouter.Poll();
      judgeProcessor.ProcessMissCheck();
  }
  ```

UniTask 보유 시 자연스럽지만 Sprint 1엔 비효율.

---

## 9. 테스트 경계 (단위 테스트 가능 지점)

- `ChartLoader.Load(jsonString) → ChartData` — pure, 가능
- `Conductor.StepToTime/TimeToStep` — pure, 가능
- `JudgeProcessor.ComputeGrade(distance) → Grade` — pure, 가능
- `ScoreSystem.ApplyNoteScore(grade) → score` — Plain Mode 한정 pure, 가능
- `ComboCounter.OnJudgment(grade) → combo` — pure, 가능
- `InputRouter` 의 angle → direction 분류 — pure, 가능

MonoBehaviour는 테스트 어려우나 위 컴포넌트들은 POCO로 분리 가능 (MonoBehaviour는 wrapper만).

---

## 10. Sprint 1 구현 순서 (의존성 기반)

1. `GameConfig` (상수) + enum 분리
2. `ChartData` (DTO) — v1 NoteType.cs salvage + noteType 제거
3. `ChartLoader` — JsonManager salvage + 단순화
4. `Conductor` — 신규
5. `Note` (runtime) + `NotePool` — v1 Note salvage + 통합 + 풀링
6. `NoteSpawner` — GamePlayer SPAWN region salvage
7. `NoteDropper` — GamePlayer DROP region salvage
8. `InputRouter` — NoteScreen + Bar salvage + 이벤트화
9. `JudgeProcessor` — 거리 기반 신규 (v1 시간 기반 폐기)
10. `ScoreSystem` — v1 ScoreManager salvage
11. `ComboCounter` — 신규
12. `HUDController` — GameScoreInfo UI 부분 salvage
13. `GameLoop` — GamePlayer 골격 salvage + 슬림화

---

## 11. 미확정 항목 (Sprint 1 시작 전 결정 권장)

✅ **2026-05-17 일괄 해소** (사용자 본인 권한 + v1 인스펙션 실측. 자세히는 `JUDGMENT_SPEC.md §4` / `DECISIONS.md`):
- `h` = 1.0 유지
- 거리 계산 기준점 = 노트 중심
- 판정선 y = **-2.5** (v1 Bar 실측)
- 라인 간격 = 1.0 unit (v1 21 lane 실측)
- 카메라 ortho size 10, pos (0, 1.5, -10) — v1 동일
- Canvas ScreenSpaceCamera, ref 956×440 — v1 동일
- HoldNote SpMiss = v1 로직 그대로 유지
- 라인 가로폭, 플릭 시간창, r 옵션, 점수/콤보/Pause UI 모두 확정

⏳ **잔존**:
- **곡 메타 spec** (BPM/Offset/AudioPath 형식) — 이재근 작성 예정. JSON 채보 드롭 검증 끝난 뒤 후순위
- **풀링 초기 사이즈**: 100 정도? 측정 후 조정

---

## 12. v1과의 핵심 차이 요약

| 항목 | v1 | v2 |
|---|---|---|
| 클래스 수 | 6 (GamePlayer 거대) | 13+ (책임 분리) |
| 통신 | public 필드 직접 접근 | 이벤트 publish/subscribe |
| 시간 단위 | 매니저마다 다름 | Conductor 단일 SoT |
| 판정 방식 | 시간 기반 (BPM 의존) | 거리 기반 (h, r) |
| 입력 상태 | bool 평행 배열 7개 | InputRouter 이벤트 |
| 풀링 | 미구현 | Pool 도입 |
| 매직 넘버 | 곳곳에 박힘 | GameConfig 집중 |
| 카드 결합도 | GamePlayer에 카드 매니저 호출 | Sprint 1엔 카드 모듈 없음 |
| 테스트 | 불가 (God class) | 컴포넌트별 가능 |

---

## 13. v1 씬 셋업 청사진 (Game.unity 실측 → v2 재현 기준)

> 상세 인스펙션: `V1_SCENE_INSPECTION.md`. 여기는 v2 씬 작성 시 직접 적용할 청사진만.

### 13.1 씬 루트 구조 (v2 재현 권장)
```
Main Camera          (Camera + AudioListener)
Canvas               (UI 컨테이너)
EventSystem          (InputSystemUIInputModule)
GameSystem           (게임 로직 컨테이너)
  ├─ NoteScreen      (pos 0, JUDGE_LINE_Y, 0; 24 Bar 부모)
  │   └─ Bar_1 ~ Bar_24
  └─ NoteBoard       (LineRenderer — hold note 폴리라인 공유 리소스)
GameLoop             (v2 신규, v1 GamePlayer 대체)
NoteCreator          (Note prefab 5종 ref)
Audio Source         (AudioSource + Conductor 컴포넌트 부착)
```

v1엔 있던 `GameUIManager` / `GameScoreInfo` / `TouchVisualizer` / `CardBoard` / `StandardCard` 는 컴포넌트로 GameLoop 또는 HUDController에 흡수.

### 13.2 Bar 셋업 패턴 (v1 동일)
- 24개 빈 GameObject + 각 Tag "Bar"
- Bar_N world position = (`-12.5 + N`, JUDGE_LINE_Y, 0) (가운데 정렬, lane 간격 LANE_WIDTH=1.0 기준)
  - v1은 21 lane이라 Bar_1 = -9, Bar_11 = +1 (center)
  - v2 24 lane은 중심 정렬 시 Bar_1 = -11.5, Bar_12.5 = 0 (또는 Bar_1 = -11 ~ Bar_24 = +12 식)
- 컴포넌트:
  - SpriteRenderer (placeholder 단계는 enabled=false. Hi-Fi 디자인 도착 시 enabled=true + bar sprite)
  - 3D `BoxCollider` (터치 입력 영역. v1과 동일하게 bar 위치보다 아래로 길게 — 플레이 영역 전체 터치)
  - `Bar` 컴포넌트 (barNum 1~24, GameLoop ref)

### 13.3 Camera 셋업 (v1 동일)
- orthographic, size 10
- position (0, 1.5, -10)
- near/far 0.3 / 1000
- background RGB(49, 77, 121), Clear Flags = Solid Color
- AudioListener 부착

### 13.4 Canvas 셋업 (v1 동일)
- Canvas component:
  - Render Mode = **ScreenSpaceCamera** (renderMode=1)
  - Render Camera = Main Camera
  - Plane Distance = 100
- CanvasScaler:
  - UI Scale Mode = Scale With Screen Size
  - Reference Resolution = 956 × 440
  - Screen Match Mode = Expand (matchWidthOrHeight = 0)
  - Reference Pixels Per Unit = 100
- Layer = UI (5)

### 13.5 UI 자식 (Sprint 1 placeholder)
- **Score** (TMP): anchor (0, 0.5) 좌측중앙, anchoredPos (25, 0), sizeDelta (100, 0) — v1 동일
- **JudgementText** (TMP): anchor stretch (0,0)–(1,1), anchoredPos (0, -50), margin 400px, fontSize 60 bold center, font `LiberationSans SDF` — v1 동일
- **Combo** (TMP): anchor (0.5, 1) **중앙 상단** (v1 우측중앙에서 변경 — 2026-05-17 사용자 결정)
- **Pause 카운트다운** (Image): `Resources/Art/UI/Pause/{1,2,3}.png` sprite 시퀀스 — v1 salvage

### 13.6 Note prefab 5종 (v1 그대로 salvage, 경로 동일)
`Assets/Resources/Prefabs/Note/{BasicNote, SlideNote, FlickNote, HoldNote, HoldNoteBody}.prefab` — `.meta` 동반 복사 필수.

### 13.7 v2에서 의식적으로 다르게 갈 것
- 콤보 UI 위치: 우측중앙 → **중앙 상단**
- 라인 수: 21 → **24** (이미 v2 spec 확정)
- 시간 소스: AudioSource.time → **AudioSettings.dspTime**
- 입력 처리: 평행 bool 배열 → InputRouter 이벤트
- 노트 클래스: 4종 단순 래퍼 → 단일 `Note` enum 기반
- 판정: 시간 기반 (BPM 의존) → 거리 기반 (h, r)
- HoldNote `noteType` 필드: 제거 (채보툴 v2)
- FlickNote 방향: 2방향 (Up/Down) → 4방향 (Up/Right/Down/Left, 시계방향)

---

## 변경 이력

- 2026-05-16: v1 코드 12개 파일 점검 후 신설. Sprint 1 기준 13개 컴포넌트 + 매핑표 + 구현 순서.
- 2026-05-17: **v1 Game.unity 인스펙션 결과 통합** — GameConfig 실측치 (JUDGE_LINE_Y -2.5, LANE_WIDTH 1.0, 카메라/Canvas 상수), §3 컴포넌트별 v1 인스펙션 노트 추가 (Conductor/NotePool/InputRouter/HUDController), §11 미확정 항목 일괄 해소 표기, §13 v1 씬 셋업 청사진 신설 (v2 씬 재현 시 직접 사용).
