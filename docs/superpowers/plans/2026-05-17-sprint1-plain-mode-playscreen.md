# Sprint 1 — Plain Mode 플레이화면 1차 구현 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Rhyffle v2의 Plain Mode 플레이화면 vertical slice — 채보 JSON 로드 → 노트 4종(Normal/Hold/Slide/Flick) 24-lane 드롭 → 거리 기반 판정(Perfect/Great/Good/Miss) → 점수(만점 1.2M) + 콤보 UI → Pause/Resume. **단일 곡 BPM 120 하드코드, 카드 시스템 제외.**

**Architecture:** 6 컴포넌트 (GameConfig 정적상수 / Conductor dspTime SoT / ChartLoader / Note 통합 enum 클래스 / JudgeProcessor 거리기반 / GameLoop 오케스트레이터). 씬 루트 6개 (Camera / Canvas / EventSystem / GameSystem / GameLoop / AudioSource — Pragmatist 권장 축소안). Sprint 1 = **load-bearing spike**: 풀링/이벤트/외부 분리 컴포넌트 미도입, Sprint 2 entry 직전 IDE refactor 직렬화. 시간 SoT = `AudioSettings.dspTime` + `AudioSource.PlayScheduled`.

**Tech Stack:** Unity 6000.2.8f1, C# / Unity Input System 1.14.2 / TextMeshPro (UGui 2.0.0) / Built-in Render Pipeline (Task 0 결정 — URP 다운그레이드) / JsonUtility (Newtonsoft 미도입) / Unity Test Framework (Edit Mode 단위 테스트). Android 빌드 타겟.

---

## 핵심 배경 (council 검증 결과 — 2026-05-17)

본 plan은 5-agent council (Steelman/Red Team/Context Keeper/Steelman 반박/Pragmatist) 2 round 심의 결과를 반영. 모든 사전 결정은 council 컨센서스 + Pragmatist의 must/즉시fix/deferred 분류표 기반.

**Sprint 1 = load-bearing spike 선언.** "Sprint 2 entry 직전 IDE refactor"가 plan에 명시되어 있으므로 코드 청결도 잣대를 spike 잣대로 재교정. Red Team Scenario D (Week 2 두 리팩토링 충돌) 는 Sprint 2 첫 주에 GameLoop 분해 → 풀링 도입 직렬화로 해소.

**진입 전 spec 박은 9개** (council 합의):
1. Conductor 초기화 = `PlayScheduled(dspTime + 0.1)` + Pause offset 누적 (Task 4)
2. PhysicsRaycaster를 Main Camera에 부착 (Task 11)
3. Note 책임 = prefab 4종(sprite/scale baked) + Note.cs (logic only) + HoldNoteBody 별개 + FlickNote prefab variant 4개 (Task 5, 6, 10)
4. Bar 콜라이더 9.6 unit 의도 = lane 입력 식별용 only, 판정 무관 (Task 8 코드 주석)
5. Bar 위치 = 중심정렬 `Bar_N world x = N - 12.5` (Bar_1=-11.5, Bar_12=-0.5, Bar_13=+0.5, Bar_24=+11.5) (Task 8, 11)
6. LANE_WIDTH = 1.0 unit (24 × 1.0 = 24 unit 폭, ortho size 10 + 16:9 화면 ~35.6 unit 안에 fit) (Task 1)
7. NoteScreen.cs 역할 = 24 Bar 컨테이너 + Bar 활성 토글 관리 (Task 9)
8. 렌더 파이프라인 = **Built-in으로 다운그레이드** (Task 0; v2 manifest 현재 URP, v1 prefab salvage 시 핑크 박스 risk 회피)
9. JSON 스키마 v0 = CHART_SPEC.md 그대로 채택 (Task 2 확인)

**Deferred to Sprint 2 entry checklist** (반드시 적어둘 것):
- Camera aspect-aware ortho size 보정 (다기종 Android 대응)
- NoteBoard LineRenderer vs HoldNoteBody LineRenderer 역할 정리

---

## File Structure

### 신규 작성 (Sprint 1)
```
RhyffleV2/
├── Packages/manifest.json                                  # URP 제거 (Task 0)
├── Assets/
│   ├── Scripts/
│   │   ├── GameConfig.cs                                   # 정적 상수 + enum (Task 1)
│   │   ├── Conductor.cs                                    # 시간 SoT (Task 4)
│   │   ├── ChartLoader.cs                                  # JSON → ChartData (Task 3)
│   │   ├── JudgeProcessor.cs                               # 거리 기반 판정 (Task 7)
│   │   ├── GameLoop.cs                                     # 오케스트레이터 (Task 9)
│   │   ├── Bar.cs                                          # 레인 anchor + collider (Task 8)
│   │   ├── NoteScreen.cs                                   # 24 Bar 컨테이너 (Task 8)
│   │   ├── Note/
│   │   │   ├── Note.cs                                     # 4종 통합 (Task 5)
│   │   │   └── HoldNoteBody.cs                             # 폴리라인 (Task 6)
│   │   └── Data/
│   │       └── ChartData.cs                                # DTO (Task 2)
│   ├── Resources/
│   │   ├── Prefabs/Note/
│   │   │   ├── BasicNote.prefab                            # v1 salvage (Task 10)
│   │   │   ├── SlideNote.prefab                            # v1 salvage (Task 10)
│   │   │   ├── FlickNote_Up.prefab                         # FlickNote variant 4종 (Task 10)
│   │   │   ├── FlickNote_Right.prefab
│   │   │   ├── FlickNote_Down.prefab
│   │   │   ├── FlickNote_Left.prefab
│   │   │   ├── HoldNote.prefab                             # v1 salvage (Task 10)
│   │   │   └── HoldNoteBody.prefab                         # v1 salvage (Task 10)
│   │   ├── Charts/
│   │   │   └── dummy_test.json                             # Week 1 더미 채보 (Task 12)
│   │   └── Art/UI/Pause/
│   │       ├── 1.png                                       # v1 salvage (Task 10)
│   │       ├── 2.png
│   │       └── 3.png
│   ├── Scenes/
│   │   └── Game.unity                                      # Sprint 1 씬 (Task 11)
│   └── Tests/
│       └── EditMode/
│           ├── Rhyffle.Tests.EditMode.asmdef               # Edit Mode 테스트 어셈블리
│           ├── GameConfigTest.cs                           # (Task 1)
│           ├── ChartLoaderTest.cs                          # (Task 3)
│           ├── ConductorTest.cs                            # (Task 4)
│           └── JudgeProcessorTest.cs                       # (Task 7)
```

### 갱신
```
.claude/context/V2_ARCHITECTURE_SPRINT1.md                  # Task 0에서 결정사항 반영
.claude/context/CURRENT_STATUS.md                           # Task 진행 시점마다 갱신
.claude/context/DECISIONS.md                                # Task 0 결정 기록
```

---

## Task 0: 진입 전 spec 확정 + URP → Built-in 다운그레이드

**Goal:** 코드 0줄 시점에 council 9개 결정을 spec 문서에 박고, v1 prefab salvage 호환성을 위해 URP 패키지 제거. 비용 5분, 안 하면 Week 1 핑크 박스 디버그 며칠 손실.

**Files:**
- Modify: `RhyffleV2/Packages/manifest.json` (URP 라인 1개 제거)
- Modify: `.claude/context/DECISIONS.md` (Task 0 결정 기록 append)
- Modify: `.claude/context/V2_ARCHITECTURE_SPRINT1.md` (§3.1 씬 셋업에 PhysicsRaycaster 추가 + 변경이력)

- [ ] **Step 1: URP 패키지 제거**

Edit `RhyffleV2/Packages/manifest.json` — `"com.unity.render-pipelines.universal": "17.2.0",` 라인 제거. 결과:
```json
{
  "dependencies": {
    "com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main",
    "com.unity.ai.navigation": "2.0.9",
    "com.unity.collab-proxy": "2.9.3",
    "com.unity.ide.rider": "3.0.38",
    "com.unity.ide.visualstudio": "2.0.23",
    "com.unity.inputsystem": "1.14.2",
    "com.unity.multiplayer.center": "1.0.0",
    "com.unity.test-framework": "1.6.0",
    "com.unity.timeline": "1.8.9",
    "com.unity.ugui": "2.0.0",
    "com.unity.visualscripting": "1.9.7",
    ... (모듈 그대로)
```

- [ ] **Step 2: Unity Editor 재시작 (또는 자동 패키지 reload 대기)**

Unity Hub에서 RhyffleV2 다시 열거나 Editor 안에서 자동 reload. Console에 "Pipeline asset missing" 등 에러 발생 가능 — Project Settings > Graphics > Scriptable Render Pipeline Settings 를 `None`으로 설정 (Built-in 활성).

- [ ] **Step 3: Built-in 활성 확인**

Unity MCP로 확인:
```
ReadMcpResource: mcpforunity://project/info
```
또는 Editor에서 Window > Rendering > Render Pipeline Converter 안 보이면 Built-in 활성 정상.

- [ ] **Step 4: DECISIONS.md에 Task 0 결정 append**

`.claude/context/DECISIONS.md` 끝에 새 entry 추가:
```markdown
## 2026-05-17: Sprint 1 진입 전 spec 9개 + URP → Built-in 다운그레이드

- **Context**: 5-agent council (Steelman/Red Team/Context Keeper/Pragmatist) 2 round 심의 결과 — 9개 진입 전 spec 박을 항목 + Sprint 1 = load-bearing spike 선언. v2 manifest가 URP 17.2.0인데 v1은 Built-in이라 prefab salvage 시 핑크 박스 risk.
- **Decision**:
  - URP 패키지 제거 → Built-in 유지. Hi-Fi 디자이너 합류 후 URP 재검토
  - Bar 위치 공식: 중심정렬 `Bar_N x = N - 12.5` (Bar_1=-11.5 ~ Bar_24=+11.5)
  - LANE_WIDTH = 1.0 unit (24 × 1.0 = 24 unit 폭)
  - NoteScreen.cs 역할 = 24 Bar 컨테이너 + Bar 활성 토글 관리
  - Bar 콜라이더 9.6 unit y = lane 입력 식별 only, 판정은 거리 기반 (코드 주석 필수)
  - Conductor 초기화 = PlayScheduled(dspTime + 0.1) + Pause offset 누적 (Scenario A 차단)
  - PhysicsRaycaster를 Main Camera에 부착 (InputSystemUIInputModule은 Canvas만)
  - Note 책임 = prefab 4종 + Note.cs logic only + HoldNoteBody 별개 + FlickNote prefab variant 4개
  - 씬 루트 8 → 6 축소 (HUDController + NoteCreator → GameLoop 흡수)
  - Sprint 1 = "load-bearing spike" 명시
- **Outcome**: V2_ARCHITECTURE_SPRINT1.md 갱신, manifest.json URP 제거, plan 작성 (docs/superpowers/plans/2026-05-17-sprint1-plain-mode-playscreen.md)
```

- [ ] **Step 5: V2_ARCHITECTURE_SPRINT1.md §3.1 갱신**

씬 루트 8개 → 6개로 표기 바꾸고 (HUDController 제거 / NoteCreator → GameLoop 흡수), Main Camera 항목에 `+ PhysicsRaycaster` 명시, Bar 항목에 콜라이더 의도 한 줄, 변경이력에 "Sprint 1 = spike 선언, 씬 6개 축소, council 진입전 9개 결정 통합" append.

- [ ] **Step 6: Commit**

```bash
git add RhyffleV2/Packages/manifest.json .claude/context/DECISIONS.md .claude/context/V2_ARCHITECTURE_SPRINT1.md docs/superpowers/plans/
git commit -m "Sprint 1 진입 전 spec 확정 + URP → Built-in 다운그레이드

- Council 2026-05-17 심의 결과 반영 (9개 진입전 spec)
- v1 prefab salvage 핑크 박스 risk 회피 위해 URP 제거
- 씬 루트 8→6 축소 (HUDController, NoteCreator → GameLoop)
- Sprint 1 = load-bearing spike 명시"
```

---

## Task 1: GameConfig 정적 상수 + enums

**Goal:** 매직 넘버 단일 집합 + 4개 enum (NoteKind / FlickDirection / JudgmentGrade / 그 외 필요 시). v1 `Define.cs` 흡수.

**Files:**
- Create: `RhyffleV2/Assets/Scripts/GameConfig.cs`
- Create: `RhyffleV2/Assets/Tests/EditMode/Rhyffle.Tests.EditMode.asmdef`
- Create: `RhyffleV2/Assets/Tests/EditMode/GameConfigTest.cs`

- [ ] **Step 1: Edit Mode 테스트 어셈블리 생성**

Create `RhyffleV2/Assets/Tests/EditMode/Rhyffle.Tests.EditMode.asmdef`:
```json
{
    "name": "Rhyffle.Tests.EditMode",
    "rootNamespace": "",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [
        "Editor"
    ],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenceIncludedAssemblies": true,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

- [ ] **Step 2: Write the failing test**

Create `RhyffleV2/Assets/Tests/EditMode/GameConfigTest.cs`:
```csharp
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
```

- [ ] **Step 3: Run test to verify it fails**

Unity Editor에서 Window > General > Test Runner > EditMode > Run All. Expected: 컴파일 에러 ("GameConfig not found").

- [ ] **Step 4: Write minimal implementation**

Create `RhyffleV2/Assets/Scripts/GameConfig.cs`:
```csharp
public static class GameConfig {
    // === Lane / Spawn 좌표 (v1 인스펙션 실측 + council 결정) ===
    public const int   LANE_COUNT       = 24;
    public const float LANE_WIDTH       = 1.0f;    // v1 21 lane Bar 간격 실측
    public const float JUDGE_LINE_Y     = -2.5f;   // v1 Bar world y 실측
    public const float SPAWN_Y          = 7.75f;   // v1 출발선 y
    public const float NOTE_HEIGHT      = 1.0f;    // h, Hi-Fi 도착 시 갱신
    // Bar_N world x 공식: N - 12.5 (중심정렬, N ∈ [1, 24] → x ∈ [-11.5, +11.5])

    // === 시간 (채보 step + BPM) ===
    public const int   STEPS_PER_QUARTER = 48;     // 1/4박자 = 48 step (CHART_SPEC.md)
    public const int   SPAWN_LEAD_STEPS  = 96;     // 노트 도착 2박자 전 spawn
    public const float DEFAULT_BPM       = 120f;   // Sprint 1 하드코드
    public const float DEFAULT_OFFSET    = 0f;
    public const float DEFAULT_SPEED     = 1.0f;   // r — Sprint 1 옵션 노출 X

    // === 카메라 / Canvas (v1 인스펙션 실측) ===
    public const float CAMERA_ORTHO_SIZE = 10.0f;
    public const float CAMERA_POS_Y      = 1.5f;
    public const float CAMERA_POS_Z      = -10.0f;
    public const int   CANVAS_REF_WIDTH  = 956;
    public const int   CANVAS_REF_HEIGHT = 440;
    public const float CANVAS_PLANE_DISTANCE = 100f;

    // === 판정 윈도우 계수 (h 단위, JUDGMENT_SPEC.md) ===
    public const float PERFECT_K = 0.2f;
    public const float GREAT_K   = 0.4f;
    public const float GOOD_K    = 0.7f;
    public const float MISS_K    = 1.0f;

    // === 점수 배율 (만점 1.2M Plain Mode) ===
    public const float PERFECT_MULT = 1.2f;
    public const float GREAT_MULT   = 1.0f;
    public const float GOOD_MULT    = 0.7f;
    public const float MISS_MULT    = 0.0f;

    // === Bar 콜라이더 (v1 실측 — lane 입력 식별 only, 판정 무관) ===
    public const float BAR_COLLIDER_CENTER_Y = -0.84f;
    public const float BAR_COLLIDER_SIZE_X   = 0.32f;
    public const float BAR_COLLIDER_SIZE_Y   = 3.0f;
    public const float BAR_COLLIDER_SIZE_Z   = 0.2f;
    public const float BAR_LOCAL_SCALE       = 3.2f;

    // === Bar 좌표 공식 헬퍼 ===
    public static float BarX(int barNum) => barNum - 12.5f;
}

public enum NoteKind { Normal, Hold, Slide, Flick }
public enum FlickDirection { Up = 0, Right = 1, Down = 2, Left = 3 }  // 시계방향
public enum JudgmentGrade { Perfect, Great, Good, Miss, SpMiss }       // SpMiss = HoldNote 마지막 떼기 (v1 salvage)
```

- [ ] **Step 5: Run test to verify it passes**

Unity Test Runner > Run All. Expected: 3 tests PASS.

- [ ] **Step 6: Commit**

```bash
git add RhyffleV2/Assets/Scripts/GameConfig.cs RhyffleV2/Assets/Tests/EditMode/
git commit -m "feat: GameConfig 정적 상수 + 3개 enum (NoteKind/FlickDirection/JudgmentGrade)

- v1 인스펙션 실측 + council 결정 반영
- BarX(N) = N - 12.5 헬퍼 (중심정렬 24 lane)
- JudgmentGrade에 SpMiss 포함 (v1 salvage)
- Edit Mode 테스트 3개"
```

---

## Task 2: ChartData DTO (JsonUtility serializable)

**Goal:** 채보 JSON 디시리얼라이즈 대상 DTO. CHART_SPEC.md §1~2 그대로. v1 `NoteType.cs` salvage하되 `noteType` 필드 제거.

**Files:**
- Create: `RhyffleV2/Assets/Scripts/Data/ChartData.cs`

- [ ] **Step 1: ChartData + 4 struct 작성**

Create `RhyffleV2/Assets/Scripts/Data/ChartData.cs`:
```csharp
using System;
using System.Collections.Generic;

[Serializable]
public class ChartData {
    public List<NormalNoteData> NormalNotes = new List<NormalNoteData>();
    public List<HoldNoteData>   HoldNotes   = new List<HoldNoteData>();
    public List<SlideNoteData>  SlideNotes  = new List<SlideNoteData>();
    public List<FlickNoteData>  FlickNotes  = new List<FlickNoteData>();
}

[Serializable] public struct NormalNoteData {
    public int position;    // chart step (3 = 1/64 박자)
    public int line;        // leftmost lane, 0~23
    public int length;      // 가로 폭 (lane 개수)
}

[Serializable] public struct HoldNoteData {
    public int position;
    public int line;
    public int count;       // hold 체인 그룹 ID (같은 count = 한 hold)
    public int length;
}

[Serializable] public struct SlideNoteData {
    public int position;
    public int line;
    public int length;
}

[Serializable] public struct FlickNoteData {
    public int position;
    public int line;
    public int length;
    public int direction;   // 0=Up, 1=Right, 2=Down, 3=Left (시계방향)
}
```

- [ ] **Step 2: Commit**

```bash
git add RhyffleV2/Assets/Scripts/Data/ChartData.cs
git commit -m "feat: ChartData DTO + 4 노트 struct (JsonUtility serializable)

- CHART_SPEC.md §1~2 그대로 구현
- v1 noteType 필드 제거 (2026-05-09 김은규 제안)
- FlickNote direction 0~3 시계방향"
```

---

## Task 3: ChartLoader — Resources.Load + JsonUtility.FromJson

**Goal:** 채보 JSON 파일 → ChartData. v1 `JsonManager`의 2단계 호출을 단일 `Load(path)` 로 정리.

**Files:**
- Create: `RhyffleV2/Assets/Scripts/ChartLoader.cs`
- Create: `RhyffleV2/Assets/Tests/EditMode/ChartLoaderTest.cs`

- [ ] **Step 1: Write the failing test**

Create `RhyffleV2/Assets/Tests/EditMode/ChartLoaderTest.cs`:
```csharp
using NUnit.Framework;

public class ChartLoaderTest {
    [Test]
    public void LoadFromJson_ParsesNormalNotes() {
        string json = @"{
            ""NormalNotes"": [
                {""position"": 48, ""line"": 0, ""length"": 1},
                {""position"": 96, ""line"": 5, ""length"": 2}
            ],
            ""HoldNotes"": [],
            ""SlideNotes"": [],
            ""FlickNotes"": []
        }";

        ChartData chart = ChartLoader.LoadFromJson(json);

        Assert.AreEqual(2, chart.NormalNotes.Count);
        Assert.AreEqual(48, chart.NormalNotes[0].position);
        Assert.AreEqual(0,  chart.NormalNotes[0].line);
        Assert.AreEqual(1,  chart.NormalNotes[0].length);
        Assert.AreEqual(96, chart.NormalNotes[1].position);
        Assert.AreEqual(5,  chart.NormalNotes[1].line);
        Assert.AreEqual(2,  chart.NormalNotes[1].length);
    }

    [Test]
    public void LoadFromJson_ParsesFlickDirection() {
        string json = @"{
            ""NormalNotes"": [],
            ""HoldNotes"": [],
            ""SlideNotes"": [],
            ""FlickNotes"": [
                {""position"": 48, ""line"": 10, ""length"": 1, ""direction"": 1}
            ]
        }";

        ChartData chart = ChartLoader.LoadFromJson(json);

        Assert.AreEqual(1, chart.FlickNotes.Count);
        Assert.AreEqual(1, chart.FlickNotes[0].direction);  // Right
    }

    [Test]
    public void LoadFromJson_EmptyChart_ReturnsZeroNotes() {
        string json = @"{""NormalNotes"":[],""HoldNotes"":[],""SlideNotes"":[],""FlickNotes"":[]}";

        ChartData chart = ChartLoader.LoadFromJson(json);

        Assert.AreEqual(0, chart.NormalNotes.Count);
        Assert.AreEqual(0, chart.HoldNotes.Count);
        Assert.AreEqual(0, chart.SlideNotes.Count);
        Assert.AreEqual(0, chart.FlickNotes.Count);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Test Runner > Run All. Expected: 컴파일 에러 ("ChartLoader not found").

- [ ] **Step 3: Write minimal implementation**

Create `RhyffleV2/Assets/Scripts/ChartLoader.cs`:
```csharp
using UnityEngine;

public static class ChartLoader {
    /// <summary>
    /// Resources/Charts/{path}.json 로드 → ChartData
    /// </summary>
    public static ChartData Load(string path) {
        TextAsset textAsset = Resources.Load<TextAsset>($"Charts/{path}");
        if (textAsset == null) {
            Debug.LogError($"[ChartLoader] Chart not found: Charts/{path}");
            return new ChartData();
        }
        return LoadFromJson(textAsset.text);
    }

    /// <summary>
    /// JSON 문자열 → ChartData (테스트용 + 핵심 로직)
    /// </summary>
    public static ChartData LoadFromJson(string json) {
        return JsonUtility.FromJson<ChartData>(json);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Test Runner > Run All. Expected: 3 ChartLoaderTest 모두 PASS.

- [ ] **Step 5: Commit**

```bash
git add RhyffleV2/Assets/Scripts/ChartLoader.cs RhyffleV2/Assets/Tests/EditMode/ChartLoaderTest.cs
git commit -m "feat: ChartLoader — Resources.Load + JsonUtility 단일 호출

- v1 JsonManager 2단계 호출 → 단일 Load(path)
- LoadFromJson(string) public 테스트용
- Edit Mode 테스트 3개 (NormalNotes parsing / FlickDirection / empty)"
```

---

## Task 4: Conductor — dspTime SoT + PlayScheduled + Pause offset 누적

**Goal:** 시간 단일 SoT. council Scenario A 차단 — `PlayScheduled(dspTime + 0.1)` 패턴 + Pause/Resume offset 누적. AudioSource 같은 GameObject에 부착 (인스펙터로 sibling lookup).

**Files:**
- Create: `RhyffleV2/Assets/Scripts/Conductor.cs`
- Create: `RhyffleV2/Assets/Tests/EditMode/ConductorTest.cs`

- [ ] **Step 1: Write the failing test (StepToTime / TimeToStep — pure)**

Create `RhyffleV2/Assets/Tests/EditMode/ConductorTest.cs`:
```csharp
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
```

- [ ] **Step 2: Run test to verify it fails**

Test Runner > Run All. Expected: 컴파일 에러.

- [ ] **Step 3: Write implementation**

Create `RhyffleV2/Assets/Scripts/Conductor.cs`:
```csharp
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
        // Android 오디오 버퍼 지연(50~150ms) 대비 0.1s 여유.
        double scheduledDsp = AudioSettings.dspTime + 0.1;
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
```

- [ ] **Step 4: Run test to verify it passes**

Test Runner > Run All. Expected: ConductorTest 3개 PASS.

- [ ] **Step 5: Commit**

```bash
git add RhyffleV2/Assets/Scripts/Conductor.cs RhyffleV2/Assets/Tests/EditMode/ConductorTest.cs
git commit -m "feat: Conductor — dspTime SoT + PlayScheduled + Pause offset 누적

- Council Scenario A 차단: PlayScheduled(dspTime + 0.1) 패턴
- Pause/Resume = pauseOffsetAccum 누적 (dspTime 자체 미정지)
- StepToTime/TimeToStep pure 메서드 + Edit Mode 테스트 3개
- [RequireComponent(typeof(AudioSource))] 강제"
```

---

## Task 5: Note 통합 클래스 (4종 enum 분기) + Reset()

**Goal:** v1 BasicNote/SlideNote/FlickNote/HoldNote 4개 단순 wrapper → 단일 `Note` MonoBehaviour. `NoteKind` enum 분기. Reset()은 **위치/판정상태/풀반환만** 처리 (시각 속성 안 건드림 — council 결정).

**Files:**
- Create: `RhyffleV2/Assets/Scripts/Note/Note.cs`

- [ ] **Step 1: Write Note 클래스**

Create `RhyffleV2/Assets/Scripts/Note/Note.cs`:
```csharp
using UnityEngine;

public class Note : MonoBehaviour {
    // === 인스턴스 상태 (Spawn 시점 set, Reset 시 clear) ===
    public NoteKind        Kind;
    public int             Position;     // chart step
    public int             Line;         // leftmost lane 0~23
    public int             Length;       // 가로 폭 (lane 개수)
    public FlickDirection? Direction;    // Flick 한정
    public bool            IsJudged;

    /// <summary>
    /// Spawn 시 호출. ChartData에서 값 받아 인스턴스 초기화.
    /// 시각 속성(sprite/scale/color)은 prefab에 baked, 여기서 안 건드림.
    /// </summary>
    public void Initialize(NoteKind kind, int position, int line, int length, FlickDirection? dir = null) {
        Kind      = kind;
        Position  = position;
        Line      = line;
        Length    = length;
        Direction = dir;
        IsJudged  = false;
    }

    /// <summary>
    /// 풀 반환 시 호출 (Sprint 1엔 Destroy 직전 호출되거나 비호출).
    /// 위치/판정 상태만 reset.
    /// </summary>
    public void ResetForPool() {
        Position = 0;
        Line     = 0;
        Length   = 0;
        Direction = null;
        IsJudged = false;
    }

    /// <summary>
    /// 곡 시간에 따라 노트 위치 갱신 (drop). GameLoop가 매 프레임 호출.
    /// </summary>
    public void UpdatePosition(float songTime, float bpm, float speed) {
        float noteTimeSec = Conductor.StepToTime(Position, bpm);
        float timeUntilHit = noteTimeSec - songTime;  // 양수 = 아직 안 도달
        // 위에서 아래로 떨어짐: y = JUDGE_LINE_Y + (timeUntilHit * speed * (SPAWN_Y - JUDGE_LINE_Y) / spawnLeadTimeSec)
        float spawnLeadTimeSec = Conductor.StepToTime(GameConfig.SPAWN_LEAD_STEPS, bpm);
        float yProgress = Mathf.Clamp01(timeUntilHit / spawnLeadTimeSec);
        float y = GameConfig.JUDGE_LINE_Y + yProgress * (GameConfig.SPAWN_Y - GameConfig.JUDGE_LINE_Y);
        float x = GameConfig.BarX(Line + 1) + (Length - 1) * GameConfig.LANE_WIDTH * 0.5f;
        transform.position = new Vector3(x, y, 0f);
    }

    /// <summary>
    /// 판정 거리 계산용 노트 중심 좌표 (world).
    /// </summary>
    public Vector2 GetCenter() {
        return new Vector2(transform.position.x, transform.position.y);
    }

    /// <summary>
    /// 이 노트가 lane을 덮는지.
    /// </summary>
    public bool ContainsLane(int lane) {
        return lane >= Line && lane < Line + Length;
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add RhyffleV2/Assets/Scripts/Note/
git commit -m "feat: Note 통합 클래스 (NoteKind enum 분기) + Initialize/Reset/UpdatePosition

- v1 BasicNote/SlideNote/FlickNote/HoldNote 4 wrapper → 단일 Note
- 시각 속성(sprite/scale)은 prefab baked, Note.cs는 logic only (council 결정)
- UpdatePosition: 위→아래 drop, JUDGE_LINE_Y ↔ SPAWN_Y 사이 보간
- GetCenter() 판정 거리 계산용"
```

---

## Task 6: HoldNoteBody — 별개 클래스 (LineRenderer 폴리라인)

**Goal:** HoldNote head 와 라이프사이클 다른 별도 클래스. LineRenderer로 사이판정 노트들 연결.

**Files:**
- Create: `RhyffleV2/Assets/Scripts/Note/HoldNoteBody.cs`

- [ ] **Step 1: Write HoldNoteBody 클래스**

Create `RhyffleV2/Assets/Scripts/Note/HoldNoteBody.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HoldNoteBody : MonoBehaviour {
    public List<Note> Keypoints = new List<Note>();
    public int CurrentKeypointIndex;
    public bool IsBroken;

    private LineRenderer line;

    void Awake() {
        line = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// HoldNote 그룹의 모든 keypoint 받아 폴리라인 시각화 초기화.
    /// </summary>
    public void Initialize(List<Note> sortedKeypoints) {
        Keypoints = sortedKeypoints;
        CurrentKeypointIndex = 0;
        IsBroken = false;
        line.positionCount = Keypoints.Count;
    }

    /// <summary>
    /// 매 프레임 keypoint 위치들로 line 갱신. GameLoop가 호출.
    /// </summary>
    public void UpdateLine(float songTime, float bpm, float speed) {
        for (int i = 0; i < Keypoints.Count; i++) {
            Keypoints[i].UpdatePosition(songTime, bpm, speed);
            line.SetPosition(i, Keypoints[i].transform.position);
        }
    }

    /// <summary>
    /// 사이판정 노트 1개 판정 처리 → CurrentKeypointIndex 진행.
    /// 8 중 4 받고 끊기면 5번째부터 fail 처리는 GameLoop/JudgeProcessor 협업.
    /// </summary>
    public void AdvanceKeypoint() {
        CurrentKeypointIndex++;
    }
}
```

- [ ] **Step 2: Commit**

```bash
git add RhyffleV2/Assets/Scripts/Note/HoldNoteBody.cs
git commit -m "feat: HoldNoteBody — LineRenderer 폴리라인 사이판정 노트 묶음

- Note 와 별개 클래스 (라이프사이클 다름, council 결정)
- [RequireComponent(typeof(LineRenderer))]
- Initialize(sortedKeypoints) + UpdateLine + AdvanceKeypoint"
```

---

## Task 7: JudgeProcessor — 거리 기반 등급 분류

**Goal:** 입력 + 활성 노트 → JudgmentGrade. 임계 = `(1 + k·r) × h`. Pure 메서드 + MonoBehaviour 컨테이너.

**Files:**
- Create: `RhyffleV2/Assets/Scripts/JudgeProcessor.cs`
- Create: `RhyffleV2/Assets/Tests/EditMode/JudgeProcessorTest.cs`

- [ ] **Step 1: Write the failing test**

Create `RhyffleV2/Assets/Tests/EditMode/JudgeProcessorTest.cs`:
```csharp
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
```

- [ ] **Step 2: Run test to verify it fails**

Test Runner > Run All. Expected: 컴파일 에러.

- [ ] **Step 3: Write implementation**

Create `RhyffleV2/Assets/Scripts/JudgeProcessor.cs`:
```csharp
using UnityEngine;

public class JudgeProcessor : MonoBehaviour {
    /// <summary>
    /// 거리(world unit) → 판정 등급. JUDGMENT_SPEC.md §1 공식.
    /// 임계 = (1 + k·r) × h
    /// </summary>
    public static JudgmentGrade ComputeGrade(float distance, float h, float r) {
        float perfect = (1f + GameConfig.PERFECT_K * r) * h;
        float great   = (1f + GameConfig.GREAT_K   * r) * h;
        float good    = (1f + GameConfig.GOOD_K    * r) * h;
        float miss    = (1f + GameConfig.MISS_K    * r) * h;

        if (distance <= perfect) return JudgmentGrade.Perfect;
        if (distance <= great)   return JudgmentGrade.Great;
        if (distance <= good)    return JudgmentGrade.Good;
        if (distance <= miss)    return JudgmentGrade.Miss;
        return JudgmentGrade.Miss;
    }

    /// <summary>
    /// 등급 → 점수 배율. JUDGMENT_SPEC.md §3.
    /// </summary>
    public static float GetScoreMultiplier(JudgmentGrade grade) {
        return grade switch {
            JudgmentGrade.Perfect => GameConfig.PERFECT_MULT,
            JudgmentGrade.Great   => GameConfig.GREAT_MULT,
            JudgmentGrade.Good    => GameConfig.GOOD_MULT,
            JudgmentGrade.Miss    => GameConfig.MISS_MULT,
            JudgmentGrade.SpMiss  => GameConfig.MISS_MULT,
            _                     => 0f,
        };
    }

    /// <summary>
    /// lane에 가장 가까운 active 노트 + 거리 반환. 없으면 null.
    /// GameLoop가 lane press 이벤트마다 호출.
    /// </summary>
    public Note FindNearestNoteInLane(int lane, System.Collections.Generic.IList<Note> activeNotes, out float distance) {
        Note nearest = null;
        distance = float.MaxValue;
        float judgeY = GameConfig.JUDGE_LINE_Y;
        foreach (var note in activeNotes) {
            if (note == null || note.IsJudged) continue;
            if (!note.ContainsLane(lane)) continue;
            float d = Mathf.Abs(note.GetCenter().y - judgeY);
            if (d < distance) {
                distance = d;
                nearest = note;
            }
        }
        return nearest;
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Test Runner > Run All. Expected: 6 JudgeProcessorTest PASS.

- [ ] **Step 5: Commit**

```bash
git add RhyffleV2/Assets/Scripts/JudgeProcessor.cs RhyffleV2/Assets/Tests/EditMode/JudgeProcessorTest.cs
git commit -m "feat: JudgeProcessor — 거리 기반 등급 분류 + 점수 배율 + 노트 탐색

- ComputeGrade(distance, h, r) — pure, JUDGMENT_SPEC.md §1 공식
- GetScoreMultiplier(grade) — pure
- FindNearestNoteInLane(lane, active) — lane press 시 GameLoop 호출
- Edit Mode 테스트 6개 (모든 등급 경계 + 점수 배율)"
```

---

## Task 8: Bar 컴포넌트 + NoteScreen 컨테이너

**Goal:** Bar = lane anchor + 3D BoxCollider (입력 식별 only — council 결정 #4). NoteScreen = 24 Bar 컨테이너 + 활성 토글 관리 (council 결정 #7).

**Files:**
- Create: `RhyffleV2/Assets/Scripts/Bar.cs`
- Create: `RhyffleV2/Assets/Scripts/NoteScreen.cs`

- [ ] **Step 1: Bar 컴포넌트 작성**

Create `RhyffleV2/Assets/Scripts/Bar.cs`:
```csharp
using UnityEngine;

/// <summary>
/// 단일 lane anchor. SpriteRenderer는 placeholder (enabled=true + 1×1 white).
/// BoxCollider 역할 = 터치 입력 lane 식별 only. 판정은 JudgeProcessor가
/// 거리 기반으로 별도 계산 (콜라이더 hit과 무관). 콜라이더 y=9.6 unit은
/// lane 전체 입력 영역 확보용 — 노트 trajectory cover 목적 아님.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class Bar : MonoBehaviour {
    public int barNum;        // 1~24
    public GameLoop gameLoop; // 인스펙터 와이어링

    /// <summary>
    /// barNum → world x. 중심정렬 24 lane: Bar_1=-11.5, Bar_24=+11.5.
    /// </summary>
    public float GetWorldX() => GameConfig.BarX(barNum);
}
```

- [ ] **Step 2: NoteScreen 컴포넌트 작성**

Create `RhyffleV2/Assets/Scripts/NoteScreen.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 24 Bar 컨테이너 + Bar 활성 토글 관리 (council 결정).
/// Sprint 1 단계엔 단순 컨테이너 + 자식 Bar 캐시. Pause 시 Bar 콜라이더 비활성화 가능.
/// </summary>
public class NoteScreen : MonoBehaviour {
    public List<Bar> bars = new List<Bar>(GameConfig.LANE_COUNT);

    void Awake() {
        // 자식 Bar 모두 캐시 (인스펙터 와이어링 누락 시 fallback)
        if (bars.Count == 0) {
            bars.AddRange(GetComponentsInChildren<Bar>());
        }
    }

    /// <summary>
    /// Pause 시 모든 Bar 콜라이더 비활성화 → 입력 무시.
    /// </summary>
    public void SetBarsActive(bool active) {
        foreach (var bar in bars) {
            var collider = bar.GetComponent<BoxCollider>();
            if (collider != null) collider.enabled = active;
        }
    }

    public Bar GetBar(int barNum) {
        foreach (var bar in bars) {
            if (bar.barNum == barNum) return bar;
        }
        return null;
    }
}
```

- [ ] **Step 3: Commit**

```bash
git add RhyffleV2/Assets/Scripts/Bar.cs RhyffleV2/Assets/Scripts/NoteScreen.cs
git commit -m "feat: Bar + NoteScreen — lane anchor + 24 Bar 컨테이너

- Bar: BoxCollider 의도 주석 (lane 식별 only, 판정 무관 — council 결정 #4)
- BarX(N) = N - 12.5 헬퍼 사용 (중심정렬)
- NoteScreen: 24 Bar 캐시 + SetBarsActive 토글 (council 결정 #7)
- [RequireComponent(typeof(BoxCollider))] Bar 강제"
```

---

## Task 9: GameLoop — 오케스트레이터 (Spawn/Drop/Input/Score/Combo 내부 메서드)

**Goal:** 6 컴포넌트의 마지막 — 진입점. v1 GamePlayer slim. Sprint 1엔 Spawn/Drop/Input/Score/Combo를 내부 메서드로 (Week 2 외부 분리). NoteCreator 흡수 (prefab ref 직접 보유).

**Files:**
- Create: `RhyffleV2/Assets/Scripts/GameLoop.cs`

- [ ] **Step 1: GameLoop 작성**

Create `RhyffleV2/Assets/Scripts/GameLoop.cs`:
```csharp
using System.Collections.Generic;
using UnityEngine;

public class GameLoop : MonoBehaviour {
    [Header("References (인스펙터 와이어링)")]
    public Conductor       conductor;
    public NoteScreen      noteScreen;
    public JudgeProcessor  judgeProcessor;
    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text comboText;
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

    [Header("Chart")]
    public string chartName = "dummy_test";

    private ChartData chart;
    private List<Note> activeNotes = new List<Note>();
    private int normalIdx, holdIdx, slideIdx, flickIdx;
    private int totalNoteCount;
    private int score;
    private int combo;
    private int baseScore;
    private bool isPaused;

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
        // 시작
        conductor.StartSong();
    }

    void Update() {
        if (isPaused) return;
        SpawnDueNotes();
        DropActiveNotes();
        ProcessInput();
        CheckMissedNotes();
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
        // 스크린 → 월드 raycast → Bar hit → lane num 추출
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f)) {
            var bar = hit.collider.GetComponent<Bar>();
            if (bar != null) {
                int lane = bar.barNum - 1;  // 0-indexed
                JudgeLanePress(lane);
            }
        }
    }

    private void JudgeLanePress(int lane) {
        var note = judgeProcessor.FindNearestNoteInLane(lane, activeNotes, out float distance);
        if (note == null) return;
        if (distance > (1f + GameConfig.MISS_K * GameConfig.DEFAULT_SPEED) * GameConfig.NOTE_HEIGHT) return;
        var grade = JudgeProcessor.ComputeGrade(distance, GameConfig.NOTE_HEIGHT, GameConfig.DEFAULT_SPEED);
        ApplyJudgment(note, grade);
    }

    // === Score + Combo (Sprint 1 내부 메서드 — Week 2 ScoreSystem/ComboCounter 분리) ===
    private void ApplyJudgment(Note note, JudgmentGrade grade) {
        note.IsJudged = true;
        int delta = Mathf.RoundToInt(baseScore * JudgeProcessor.GetScoreMultiplier(grade));
        score += delta;
        if (grade == JudgmentGrade.Miss) combo = 0;
        else combo++;
        UpdateHUD(grade);
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
```

- [ ] **Step 2: Commit**

```bash
git add RhyffleV2/Assets/Scripts/GameLoop.cs
git commit -m "feat: GameLoop — Sprint 1 오케스트레이터 (Spawn/Drop/Input/Score/Combo 내부 메서드)

- 6 컴포넌트의 마지막 — v1 GamePlayer slim 버전
- NoteCreator 흡수 (prefab ref 직접 보유 — council 권장 6 루트 축소)
- Unity Input System (Touchscreen + Mouse fallback)
- Physics.Raycast로 Bar hit → lane 추출
- Sprint 1 = load-bearing spike: 풀링/이벤트 미도입, Week 2 외부 분리
- TODO: HoldNote group 처리 / Pause 카운트다운 — Week 2"
```

---

## Task 10: v1 prefab + sprite salvage

**Goal:** Note prefab 5종 + Pause sprite 3장을 v1 → v2로 `.meta` 동반 복사. FlickNote는 4방향 prefab variant 만들기 (rotation baked).

**Files:**
- Copy: 8개 prefab + 3 sprite from v1 to v2 Resources

- [ ] **Step 1: v1 → v2 Note prefab 5종 복사 (`.meta` 동반)**

```bash
mkdir -p "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/BasicNote.prefab"          "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/BasicNote.prefab.meta"     "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/SlideNote.prefab"          "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/SlideNote.prefab.meta"     "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/FlickNote.prefab"          "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/FlickNote.prefab.meta"     "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/HoldNote.prefab"           "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/HoldNote.prefab.meta"      "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/HoldNoteBody.prefab"       "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Prefabs/Note/HoldNoteBody.prefab.meta"  "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Prefabs/Note/"
```

- [ ] **Step 2: Pause sprite 3장 복사 (`.meta` 동반)**

```bash
mkdir -p "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Art/UI/Pause"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Art/UI/Pause/1.png"      "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Art/UI/Pause/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Art/UI/Pause/1.png.meta" "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Art/UI/Pause/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Art/UI/Pause/2.png"      "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Art/UI/Pause/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Art/UI/Pause/2.png.meta" "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Art/UI/Pause/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Art/UI/Pause/3.png"      "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Art/UI/Pause/"
cp "/c/Users/ljk91/OneDrive/문서/Github/Rhyffle/Assets/Resources/Art/UI/Pause/3.png.meta" "/c/Users/ljk91/Onedrive/문서/github/RhyffleV2/Rhyffle/RhyffleV2/Assets/Resources/Art/UI/Pause/"
```

- [ ] **Step 3: Unity Editor 재시작 + 컴파일 확인**

```
ReadMcpResource: mcpforunity://editor/state
```
Expected: `isCompiling: false`, `compilation.is_domain_reload_pending: false`. Console 에러 없음 (특히 missing script ref).

만약 prefab의 v1 스크립트 ref가 missing이면 (v1엔 BasicNote.cs/SlideNote.cs/FlickNote.cs/HoldNote.cs 등 wrapper 있었음), 각 prefab 열어서 **Note 컴포넌트 추가** + missing script 제거. Editor 수동 작업 1개당 1~2분.

- [ ] **Step 4: FlickNote 4방향 variant 생성 (Editor 수동)**

`FlickNote.prefab` 을 base로:
- `FlickNote_Up.prefab` (Z rotation 0)
- `FlickNote_Right.prefab` (Z rotation -90)
- `FlickNote_Down.prefab` (Z rotation 180)
- `FlickNote_Left.prefab` (Z rotation 90)

Unity Editor: Project 창에서 FlickNote.prefab 4번 duplicate + 이름 변경 + 각각 열어서 root Transform rotation 설정 + save. 약 5분.

- [ ] **Step 5: Commit**

```bash
git add RhyffleV2/Assets/Resources/
git commit -m "chore: v1 → v2 prefab/sprite salvage

- Note prefab 5종 (.meta 동반): BasicNote/SlideNote/FlickNote/HoldNote/HoldNoteBody
- Pause sprite 3장 (.meta 동반): 1.png/2.png/3.png
- FlickNote 4방향 variant 추가 (rotation baked)
- council 결정: prefab = visual baked, Note.cs = logic only"
```

---

## Task 11: Game.unity 씬 셋업 (Unity MCP 자동화)

**Goal:** 씬 루트 6개 (council 권장 축소안). Unity MCP for Unity로 자동화. 24 Bar 배치, Camera/Canvas/AudioSource 셋업 + PhysicsRaycaster 부착.

**Files:**
- Create: `RhyffleV2/Assets/Scenes/Game.unity`

- [ ] **Step 1: 새 씬 생성**

Unity MCP:
```
mcp__unity-v2__manage_scene
  action: create
  name: Game
  path: Assets/Scenes/Game.unity
```
또는 Editor: File > New Scene > Basic 2D > Save As "Assets/Scenes/Game.unity".

- [ ] **Step 2: Main Camera 셋업**

씬 루트에 이미 있는 Main Camera 인스펙터:
- Transform: position (0, 1.5, -10), rotation (0,0,0), scale (1,1,1)
- Camera 컴포넌트: Projection = Orthographic, Size = 10, Clear Flags = Solid Color, Background = RGB(49,77,121,0), Clipping Planes Near=0.3, Far=1000
- AudioListener: 이미 부착됨 (Unity 기본)
- **+ PhysicsRaycaster 컴포넌트 추가** (council 결정 #2). Inspector > Add Component > Event > Physics Raycaster.

Unity MCP:
```
mcp__unity-v2__manage_components
  action: add
  gameobjectId: <Main Camera id>
  componentType: UnityEngine.EventSystems.PhysicsRaycaster
```

- [ ] **Step 3: Canvas 셋업**

GameObject > UI > Canvas. 인스펙터:
- Canvas: Render Mode = Screen Space - Camera, Render Camera = Main Camera, Plane Distance = 100
- CanvasScaler: UI Scale Mode = Scale With Screen Size, Reference Resolution = (956, 440), Screen Match Mode = Expand, Match = 0
- GraphicRaycaster: 기본

Canvas 자식 4개 생성:
- **Score** (UI > Text - TextMeshPro): anchor (0, 0.5) 좌측중앙, anchoredPosition (25, 0), sizeDelta (200, 50), text "0", fontSize 30
- **JudgementText** (UI > Text - TextMeshPro): anchor stretch (0,0)-(1,1), anchoredPosition (0, -50), margin 400px, fontSize 60, fontStyle Bold, alignment Center, font LiberationSans SDF, text ""
- **Combo** (UI > Text - TextMeshPro): anchor (0.5, 1) **중앙 상단** (council 결정 — v1 우측중앙에서 변경), anchoredPosition (0, -50), sizeDelta (200, 80), fontSize 50, alignment Center, text ""
- **PauseCountdown** (UI > Image): anchor (0.5, 0.5) 중앙, sizeDelta (200, 200), Source Image = (Pause sprite 1.png 임의), disable by default (Week 2에 활성 로직)

- [ ] **Step 4: EventSystem 확인**

Canvas 추가 시 자동 생성됨. 인스펙터 확인:
- InputSystemUIInputModule 부착되어 있어야 함 (legacy StandaloneInputModule 아님)
- 없으면 GameObject > UI > Event System.

- [ ] **Step 5: GameSystem + NoteScreen + 24 Bar 배치**

빈 GameObject `GameSystem` 생성 (pos 0,0,0).
자식으로 빈 GameObject `NoteScreen` 생성 (pos 0,-2.5,0) → **NoteScreen 컴포넌트** 추가.

NoteScreen 아래에 **24개 Bar** 생성. 각 Bar (Unity MCP 또는 Editor batch):
- 이름 `Bar_{N}` (N = 1~24)
- Tag = "Bar" (Tag Manager에서 신규 생성 필요 시 추가)
- Layer = Default
- Transform: localPosition (`N - 12.5`, 0, 0), localScale (3.2, 3.2, 1)
- **SpriteRenderer 컴포넌트**: enabled = **true** (Sprint 1 디버그 가시화 — Pragmatist 권장. 1×1 white sprite 사용. Sprite > Knob 또는 새로 1x1 white 만들기), Color = (1,1,1,0.3)
- **3D BoxCollider 컴포넌트** (BoxCollider2D 아님!): Center (0, -0.84, 0), Size (0.32, 3.0, 0.2), Is Trigger = false
- **Bar 컴포넌트**: barNum = N

24개를 hand-place는 3-5h 노가다 risk (Pragmatist underestimated #1). 권장: Unity MCP `execute_code` 로 1회성 셋업 스크립트:
```csharp
// Unity Editor 메뉴에서 호출하거나 Unity MCP execute_code로 1회 실행
[UnityEditor.MenuItem("Rhyffle/Setup 24 Bars")]
static void SetupBars() {
    var noteScreen = GameObject.Find("NoteScreen");
    var sprite = UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
    for (int n = 1; n <= 24; n++) {
        var bar = new GameObject($"Bar_{n}");
        bar.transform.SetParent(noteScreen.transform, false);
        bar.transform.localPosition = new Vector3(n - 12.5f, 0, 0);
        bar.transform.localScale = new Vector3(3.2f, 3.2f, 1);
        bar.tag = "Bar";

        var sr = bar.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1, 1, 1, 0.3f);
        sr.sortingOrder = 10;

        var col = bar.AddComponent<BoxCollider>();
        col.center = new Vector3(0, -0.84f, 0);
        col.size = new Vector3(0.32f, 3.0f, 0.2f);

        var barComp = bar.AddComponent<Bar>();
        barComp.barNum = n;
    }
    Debug.Log("24 Bars created");
}
```

Editor에서 `Rhyffle/Setup 24 Bars` 메뉴 실행 → 24 Bar 자동 생성. Tag "Bar" 가 Tag Manager에 없으면 먼저 추가.

- [ ] **Step 6: NoteBoard 생성**

GameSystem 아래에 빈 GameObject `NoteBoard` (pos 0,0,0). LineRenderer 컴포넌트 추가 (Sprint 1엔 unused, Week 2 Hold polyline에서 사용).

- [ ] **Step 7: GameLoop GameObject 생성**

씬 루트에 빈 GameObject `GameLoop`. **GameLoop 컴포넌트** 추가. 인스펙터 와이어링:
- conductor → (Audio Source GameObject 의 Conductor)
- noteScreen → (NoteScreen GameObject)
- judgeProcessor → (GameLoop 자신에 JudgeProcessor 컴포넌트도 추가 후 drag)
- scoreText → (Canvas > Score)
- comboText → (Canvas > Combo)
- judgementText → (Canvas > JudgementText)
- basicNotePrefab → `Resources/Prefabs/Note/BasicNote.prefab`
- slideNotePrefab → SlideNote
- flickNoteUpPrefab ~ flickNoteLeftPrefab → 4 variants
- holdNotePrefab → HoldNote
- holdNoteBodyPrefab → HoldNoteBody
- chartName → `"dummy_test"`

- [ ] **Step 8: Audio Source GameObject 생성**

씬 루트에 빈 GameObject `Audio Source`. **AudioSource 컴포넌트** 추가:
- AudioClip: 비워둠 (Sprint 1 음원 후순위 — dspTime 가상 시계로 검증)
- Volume = 0.3, Pitch = 1, Spatial Blend = 0 (2D), Play On Awake = **false**, Loop = false

**Conductor 컴포넌트 추가** (RequireComponent으로 자동) — bpm = 120, offset = 0.

GameLoop 인스펙터에서 conductor → Audio Source 의 Conductor로 drag.

- [ ] **Step 9: Tag "Bar" 추가**

Edit > Project Settings > Tags and Layers > Tags에 "Bar" 추가. (Bar GameObject들이 이걸 사용)

- [ ] **Step 10: 씬 저장**

```
mcp__unity-v2__manage_scene
  action: save
```
또는 Editor: File > Save (Ctrl+S).

- [ ] **Step 11: Commit**

```bash
git add RhyffleV2/Assets/Scenes/Game.unity RhyffleV2/ProjectSettings/TagManager.asset
git commit -m "feat: Game.unity 씬 셋업 — 6 루트 (Pragmatist 축소안)

- Main Camera (Ortho 10, +PhysicsRaycaster — council #2)
- Canvas (ScreenSpaceCamera 956×440, Score/JudgementText/Combo 중앙상단/PauseCountdown)
- EventSystem (InputSystemUIInputModule)
- GameSystem (NoteScreen + 24 Bar + NoteBoard)
- GameLoop (인스펙터 와이어링 완료)
- Audio Source (+ Conductor)
- Tag 'Bar' 추가"
```

---

## Task 12: 더미 채보 JSON 작성

**Goal:** Week 1 step 10. 최소 채보 — 4종 노트 각 1개씩 + 24 lane 일부 활용. JSON 드롭 시각 검증용.

**Files:**
- Create: `RhyffleV2/Assets/Resources/Charts/dummy_test.json`

- [ ] **Step 1: 더미 채보 JSON 작성**

Create `RhyffleV2/Assets/Resources/Charts/dummy_test.json`:
```json
{
    "NormalNotes": [
        {"position": 96,  "line": 0,  "length": 1},
        {"position": 144, "line": 3,  "length": 1},
        {"position": 192, "line": 6,  "length": 1},
        {"position": 240, "line": 9,  "length": 1},
        {"position": 288, "line": 12, "length": 1},
        {"position": 336, "line": 15, "length": 1},
        {"position": 384, "line": 18, "length": 1},
        {"position": 432, "line": 21, "length": 1}
    ],
    "HoldNotes": [
        {"position": 480, "line": 0,  "count": 1, "length": 1},
        {"position": 528, "line": 0,  "count": 1, "length": 1},
        {"position": 576, "line": 0,  "count": 1, "length": 1}
    ],
    "SlideNotes": [
        {"position": 624, "line": 5,  "length": 3}
    ],
    "FlickNotes": [
        {"position": 672, "line": 10, "length": 1, "direction": 0},
        {"position": 720, "line": 12, "length": 1, "direction": 1},
        {"position": 768, "line": 14, "length": 1, "direction": 2},
        {"position": 816, "line": 16, "length": 1, "direction": 3}
    ]
}
```
설명: BPM 120 기준 (1 beat = 48 step = 0.5초). position 96 = 2 beat = 1초 후 첫 노트. 모든 4종 노트 + 4방향 flick 포함. 총 약 17초 분량.

- [ ] **Step 2: Commit**

```bash
git add RhyffleV2/Assets/Resources/Charts/dummy_test.json
git commit -m "feat: dummy_test.json — Week 1 더미 채보 (4종 노트 + 4방향 flick)

- BPM 120 기준 17초 분량
- NormalNotes 8개 (lane 0~21 분포)
- HoldNotes 3개 (count=1 그룹)
- SlideNotes 1개 (length=3)
- FlickNotes 4방향 각 1개"
```

---

## Task 13: 통합 검증 + Steelman Week 1 게이트 3개

**Goal:** Sprint 1 DoD 항목별 수동 검증 + Steelman이 제안한 디바이스 검증 게이트 3개 (dspTime drift / Bar raycast / Canvas z-fight).

**Files:** (없음 — 검증 단계)

- [ ] **Step 1: 컴파일 + Console clean 확인**

Unity MCP:
```
ReadMcpResource: mcpforunity://editor/state
mcp__unity-v2__read_console (errors only)
```
Expected: isCompiling=false, error 0. Warning은 허용.

- [ ] **Step 2: Edit Mode 테스트 전수 통과**

Unity Test Runner > EditMode > Run All. Expected: 모든 테스트 PASS (Task 1/3/4/7 합계 ~15개).

- [ ] **Step 3: Play Mode 진입 — JSON 채보 드롭 시각 검증 (우선순위 1)**

Unity Editor에서 Play. 약 1초 후 첫 노트가 Bar_1 위치에 떨어지기 시작 → 17초 동안 모든 노트 떨어지는지 시각 확인.

기대 결과:
- 노트가 화면 상단(y≈7.75)에서 판정선(y≈-2.5)으로 떨어짐
- 각 노트가 올바른 lane (Bar_1 = -11.5, Bar_24 = +11.5) 에 위치
- HoldNote 3개는 같은 count=1 그룹이라 같은 lane (0번 = Bar_1)에 연속

문제 발견 시 즉시 fix:
- 노트 안 보임 → Bar SpriteRenderer enabled=true 확인 + Sprite 할당 / 또는 노트 prefab의 SpriteRenderer 확인
- 노트 위치 어긋남 → GameConfig.BarX 또는 Note.UpdatePosition 검토
- 컴파일 안 됨 → Bar Tag 추가 확인

- [ ] **Step 4: Steelman 게이트 1 — dspTime drift 검증**

테스트 코드 (Play Mode test 또는 임시 디버그 스크립트):
```csharp
// GameLoop의 Conductor를 60초간 관찰
// 매 1초 SongTime 로깅, 60초 후 SongTime이 60.0 ± 0.017 (1 frame) 내인지 확인
```
Editor 60초 재생 후 Console SongTime 확인. Expected: 60.0초 시점 SongTime ≈ 59.98~60.02. 1프레임 초과 drift면 Conductor 로직 재검토.

- [ ] **Step 5: Steelman 게이트 2 — 24 Bar Raycast hit rate**

Editor Play 중 화면 좌측 Bar_1 영역 클릭 → Bar_1 hit (lane=0) 확인. 우측 Bar_24 영역 클릭 → Bar_24 hit (lane=23). 인접 Bar 경계 (예: Bar_12 ↔ Bar_13) 정확히 클릭하여 오인식 없는지 확인. Debug.Log 추가 권장.

Expected: 모든 클릭이 의도한 Bar에 hit. 오인식 발견 시 BoxCollider size 또는 Camera 위치 재검토.

- [ ] **Step 6: Steelman 게이트 3 — Canvas + 노트 z-fight 확인**

Play Mode에서 노트가 떨어지는 동안 UI Text (Score / JudgementText / Combo) 와 노트가 겹치지 않거나, 겹치더라도 UI가 항상 노트 위로 표시되는지 시각 확인.

Expected: Canvas planeDistance 100 + 노트 z=0 → UI가 항상 노트 위. 만약 노트가 UI 위로 튀면 Canvas 모드 재확인 (ScreenSpaceCamera + Render Camera = Main Camera 여야 함).

- [ ] **Step 7: 판정 동작 확인**

Editor Play, 떨어지는 노트 4종 각각에 대해 적절한 타이밍 클릭. Console에 JudgementGrade 로그 + UI JudgementText 갱신 확인.
- 정확한 타이밍 → "Perfect"
- 살짝 늦/이르 → "Great" / "Good"
- 한참 빗나감 → "Miss"

Expected: 각 등급이 거리 임계대로 분류됨. Score/Combo UI 갱신.

- [ ] **Step 8: Pause/Resume 동작 확인**

Play 중 GameLoop의 Pause() 메서드 호출 (Inspector > Right click > GameLoop > Pause 또는 임시 키 바인딩). Expected: 노트 멈춤, conductor.SongTime 멈춤. Resume() 호출 시 정상 진행.

- [ ] **Step 9: DoD 체크리스트 검증**

V2_ARCHITECTURE_SPRINT1.md §7 Definition of Done 항목별 PASS/FAIL 기록:
- [ ] 채보 JSON 1개 로드 가능
- [ ] 노트 4종 화면에 떨어짐
- [ ] 4종 노트 모두 판정 가능 (Perfect/Great/Good/Miss)
- [ ] Plain Mode 점수 계산 + 만점 1.2M (모두 Perfect 시)
- [ ] 콤보 카운터 UI 표시
- [ ] 곡 재생 + 채보 동기화 (Sprint 1엔 음원 없이 dspTime 가상 시계 OK)
- [ ] 일시정지 / 재개 동작

- [ ] **Step 10: CURRENT_STATUS.md 갱신**

Sprint 1 완료 표시 + 다음 액션 = "Sprint 2 entry 직전 IDE refactor (NotePool, C# event, 5개 컴포넌트 분리, Camera aspect-aware, HoldNoteBody/NoteBoard LineRenderer 정리)".

- [ ] **Step 11: Commit**

```bash
git add .claude/context/CURRENT_STATUS.md
git commit -m "Sprint 1 통합 검증 완료 + DoD 체크 + Steelman 게이트 3개

- Edit Mode 테스트 15개 PASS
- JSON 드롭 시각 검증 (4종 노트 17초 채보)
- dspTime drift 60초 측정
- 24 Bar Raycast 인접 hit 정확
- Canvas z-fight 없음
- Pause/Resume 동작
- Sprint 1 = load-bearing spike 완료
- 다음: Sprint 2 entry IDE refactor"
```

---

## Self-Review

**1. Spec coverage:**
- ✅ V2_ARCHITECTURE_SPRINT1.md 6 컴포넌트 모두 Task 1~9에 매핑
- ✅ 씬 셋업 §3.1 → Task 11 (Pragmatist 축소 6 루트 반영)
- ✅ Council 진입 전 9개 → Task 0 (URP 다운그레이드, DECISIONS append, §3.1 갱신) + 각 Task에 baked
- ✅ Steelman Week 1 게이트 3개 → Task 13 Step 4~6
- ✅ DoD 7개 항목 → Task 13 Step 9
- ⚠️ Gap: HoldNote 그룹 처리 (count 묶음 + 폴리라인) Sprint 1 Week 2로 표시. 더미 채보엔 단순 keypoint 3개라 시각 확인만 가능. 본격 hold 판정은 Week 2 작업

**2. Placeholder scan:**
- "TODO: HoldNote group 처리 — Week 2" Task 9 GameLoop 마지막에 1개 (의도적 Sprint 1 외)
- "TODO: Pause sprite 3-2-1 카운트다운 (Week 2 후반)" Task 9 Resume() (의도적 Week 2)
- 그 외 placeholder 없음 — 모든 step에 실행 가능한 코드/명령 명시

**3. Type consistency:**
- ✅ ChartData, NormalNoteData/HoldNoteData/SlideNoteData/FlickNoteData (Task 2)
- ✅ NoteKind (Normal/Hold/Slide/Flick), FlickDirection (Up=0/Right=1/Down=2/Left=3), JudgmentGrade (Perfect/Great/Good/Miss/SpMiss) (Task 1) — Task 5/7/9 사용 일치
- ✅ Conductor.SongTime, CurrentStep, StartSong/Pause/Resume — Task 4 정의, Task 9 사용 일치
- ✅ Note.Initialize/ResetForPool/UpdatePosition/GetCenter/ContainsLane — Task 5 정의, Task 7/9 사용 일치
- ✅ JudgeProcessor.ComputeGrade/GetScoreMultiplier/FindNearestNoteInLane — Task 7 정의, Task 9 사용 일치
- ✅ Bar.barNum/GetWorldX, NoteScreen.bars/SetBarsActive/GetBar — Task 8 정의, Task 9/11 사용 일치
- ✅ GameConfig 상수/BarX — Task 1 정의, 모든 Task 사용 일치

---

## Execution Handoff

**Plan complete and saved to `docs/superpowers/plans/2026-05-17-sprint1-plain-mode-playscreen.md`. Two execution options:**

**1. Subagent-Driven (recommended)** - 새 subagent를 Task 별로 dispatch, Task 사이 review, 빠른 iteration

**2. Inline Execution** - 이 세션에서 executing-plans 사용, checkpoint별 batch 실행

**Which approach?**

**If Subagent-Driven chosen:**
- REQUIRED SUB-SKILL: superpowers:subagent-driven-development
- 각 Task 별 fresh subagent + two-stage review

**If Inline Execution chosen:**
- REQUIRED SUB-SKILL: superpowers:executing-plans
- Batch 실행 + review checkpoint
