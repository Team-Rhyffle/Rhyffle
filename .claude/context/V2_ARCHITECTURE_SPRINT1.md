# Rhyffle v2 — Sprint 260514 진입 아키텍처

> **Sprint 1 한정 entry plan.** 도착점 청사진은 `V2_ARCHITECTURE.md`.
> Council (2026-05-16) 결정: 14개 일제 분해 대신 **6개 점진 분해 + vertical slice**.
> 첫 1주 동작 보장 우선, 이후 작동 코드에서 IDE refactor 로 분해 확장.

---

## 1. 결정 배경 (요약)

V2_ARCHITECTURE.md (14 컴포넌트) 가 Sprint 1 학생 1인 진입점으로 과설계임 — 5명 의회 (Steelman / Red Team / Context Keeper / Pragmatist / Constraint Challenger) 합의:

- 14개 일제 분해 = 35~60h 코딩 (Unity 셋업/prefab/이벤트 와이어링/디버깅 포함). Sprint 1 데드라인 위험
- 6개 점진 분해 = 20~35h. 1주차에 노트 떨어지고 판정 붙는 동작 보장
- 풀링/이벤트/HUDController/ComboCounter 분리는 Sprint 2 진입 전 작업 (작동 코드 refactor — 안전)
- 청사진 자체는 **도착점**으로 유지

상세: `DECISIONS.md` 2026-05-16 항목

---

## 2. Sprint 1 컴포넌트 (6개)

```
GameConfig (상수) ──┐
                    ↓
ChartLoader → ChartData → GameLoop
                            │
Conductor ──────────────────┤
                            │
                            ├─ [내부 메서드] Spawn (timing 따른 Pool.Get... but no pool yet)
                            ├─ [내부 메서드] Drop (모든 Note.UpdatePosition)
                            ├─ [내부 메서드] Input (Touch + Editor Mouse)
                            ├─ [내부 메서드] Score (직접 Managers.Score.ApplyNoteScore)
                            └─ [내부 메서드] Combo (단순 int + UI 직접 갱신)

JudgeProcessor (거리 기반 판정) ← GameLoop 가 호출
Note (4종 통합 클래스, enum 기반)
```

### 2.1 `GameConfig`
- 매직 넘버 단일 집합
- Sprint 1 placeholder 값들 (h, 판정선 등) 명시 + "갱신 가능" 주석
- enum: `NoteKind`, `FlickDirection`, `JudgmentGrade`

### 2.2 `Conductor`
- 곡 시간 단일 SoT (`SongTime`, `CurrentStep`)
- `AudioSettings.dspTime` 사용 (AudioSource.time jitter 회피 — Pragmatist 지적)
- BPM/Offset 하드코드로 시작 (곡 메타 spec 도착 전)
- `Pause()` / `Resume()` 기본

### 2.3 `ChartLoader`
- `Load(path) → ChartData` 단일 호출 (v1 2단계 호출 정리)
- Resources.Load 우선, persistentDataPath fallback
- HoldNote `noteType` 필드 없음 (v2 spec)
- ChartData = NormalNotes/HoldNotes/SlideNotes/FlickNotes 4종 배열

### 2.4 `Note` (통합 런타임 클래스)
- v1 BasicNote/SlideNote/FlickNote/HoldNote 통합 (단순 래퍼 제거)
- `NoteKind` enum 으로 분기
- 필드: `Kind`, `Position`, `Line` (0-23), `Length`, `Direction?` (Flick 한정)
- `void UpdatePosition(float songTime, float speed)` — 자기 위치 갱신
- `Vector2 GetCenter()` — 판정 위치
- `HoldNoteBody` 는 별도 (폴리라인 LineRenderer + Keypoints 리스트)

### 2.5 `JudgeProcessor` (Sprint 1 신규)
- 입력 + 활성 노트 → `JudgmentResult` 계산
- 거리 기반: `distance = |note.GetCenter().y - JUDGE_LINE_Y|`
- 등급: `(1 + k·r) · h` 임계
- HoldNote: 사이 판정 노트별 순차 (curJudge 인덱스)
- FlickNote: 거리 조건 + 방향 일치
- `JudgmentResult` struct: `Grade`, `Lane`, `IsFirstNote` (Sprint 2 진입 시 reserve field 추가)

### 2.6 `GameLoop` (Orchestrator — 진입점)
- v1 GamePlayer 의 슬림화 버전
- 14컴포넌트 청사진의 Spawn/Drop/Input/Score/Combo 를 **내부 메서드로 시작**
- 풀링 미도입 (Instantiate/Destroy)
- 이벤트 시스템 미도입 (직접 함수 호출)
- 24-lane 배열 (`bool[24]` 등) — v1의 21 → 24 만 변경
- 카드 매니저 호출 제거 (`Managers.Card.ResetCards()` 등)
- UniTask vs Update: **Update + bool isPaused** (단순화)

---

## 3. Week 1 / Week 2 / Sprint 2 entry

### Week 1 (목표: 화면에 노트 떨어지고 판정 붙음)

> **우선순위 (2026-05-17)**: 음원/곡 메타 통합은 **후순위**. JSON 채보 로드 → 노트 4종이 올바른 line/length/timing으로 떨어지는지 시각 검증부터. Conductor는 dspTime 가상 시계로 시작(오디오 X), 곡 메타 spec은 JSON 드롭 검증 끝나고 작성.

| 순서 | 작업 | 추정 |
|---|---|---|
| 1 | `GameConfig` 신규 + enum 분리 (`Define.cs` 정리) | 2h |
| 2 | `ChartData` (DTO) — v1 NoteType.cs salvage + noteType 제거 + 클래스명 정리 | 1h |
| 3 | `ChartLoader` — v1 JsonManager salvage + Load 단일 호출 | 1h |
| 4 | `Conductor` 신규 — dspTime, BPM 하드코드, Pause/Resume | 3h |
| 5 | `Note` (통합 클래스) — v1 Note.cs salvage + 단순 래퍼 4개 통합 + Reset() 추가 | 3h |
| 6 | `HoldNoteBody` — v1 salvage + GC 최적화 (widthCurve 캐싱) | 2h |
| 7 | `JudgeProcessor` 신규 — 거리 기반 임계 함수, Grade 분류 | 3h |
| 8 | `GameLoop` — v1 GamePlayer 핵심 골격 + Spawn/Drop 내부 메서드 + 카드 호출 제거 | 4h |
| 9 | **Unity 씬 셋업** — v1 `Game.unity` 청사진 재현 (아래 §3.1 참조) | 4h |
| 10 | 첫 채보 더미 데이터 (Tap 위주, 모든 노트 종류 1개씩) — 직접 작성 or 김민주 요청 | 2h |
| 11 | 통합 테스트 (노트 떨어짐 + Tap 판정) | 4~8h |

#### 3.1 Week 1 step 9 — Unity 씬 셋업 상세 (v1 인스펙션 기반)

> 출처: `V1_SCENE_INSPECTION.md`. v1 Game.unity 인스펙터 값 그대로 v2에 재현. spec 추측 없음.

**씬 루트 (6개 — council Pragmatist 권장 축소안. HUDController + NoteCreator → GameLoop 흡수)**:
1. **Main Camera** — Orthographic, size 10, pos (0, 1.5, -10), near/far 0.3/1000, bg RGB(49,77,121), Clear Flags Solid Color, AudioListener 부착. **+ PhysicsRaycaster** (council 결정 #2 — `InputSystemUIInputModule`은 Canvas raycast 모듈 only, World BoxCollider hit은 별도 Physics Raycaster 필요)
2. **Canvas** — Render Mode ScreenSpaceCamera, Render Camera = Main Camera, Plane Distance 100. CanvasScaler ScaleWithScreenSize, ref res 956×440, ScreenMatchMode Expand
3. **EventSystem** — `InputSystemUIInputModule` 컴포넌트 (legacy `StandaloneInputModule` 아님)
4. **GameSystem** (빈 컨테이너)
   - 자식 **NoteScreen** (pos 0, -2.5, 0) — `NoteScreen` 컴포넌트 + 24 Bar 자식
   - 자식 **NoteBoard** (pos 0, 0, 0) — `LineRenderer` 컴포넌트 (hold polyline 공유)
5. **GameLoop** — v2 신규. `GameLoop` 컴포넌트 + Note prefab 5종 ref 직접 보유 (NoteCreator 흡수)
6. **Audio Source** — `AudioSource` 컴포넌트 (vol 0.3, pitch 1, spatialBlend 0, playOnAwake false, loop false) + `Conductor` 컴포넌트. Sprint 1엔 clip 없음

**Bar 배치 (24개) — 중심정렬 확정 (council 결정 #5)**:
- Bar_N world position = (`N - 12.5`, -2.5, 0). Bar_1 = -11.5, Bar_12 = -0.5, Bar_13 = +0.5, Bar_24 = +11.5
- 화면 가로 폭 = 23 unit (Bar_1 ~ Bar_24), 카메라 시야 (ortho 10 × aspect 16:9) = ~35.6 unit 안에 fit
- 각 Bar:
  - Tag = "Bar", Layer = Default
  - SpriteRenderer **enabled=true + 1×1 white α=0.3 placeholder** (Sprint 1 디버깅 가시화 — Pragmatist 권장. Hi-Fi 도착 시 디자인 sprite로 교체)
  - 3D **BoxCollider** (BoxCollider2D 아님!): center (0, -0.84, 0), size (0.32, 3.0, 0.2), localScale 3.2 — v1 동일
  - **콜라이더 의도 (council 결정 #4)**: y 9.6 unit = lane 전체 입력 영역 확보용. 판정과 무관 — JudgeProcessor가 거리 기반으로 별도 계산
  - `Bar` 컴포넌트: barNum (1~24), GameLoop ref

**UI 자식 (Canvas)**:
- **Score** TMP: anchor (0, 0.5) 좌측중앙, anchoredPos (25, 0), sizeDelta (100, 0)
- **JudgementText** TMP: anchor stretch (0,0)–(1,1), anchoredPos (0, -50), margin 400px, fontSize 60 bold center, font `LiberationSans SDF`
- **Combo** TMP: anchor (0.5, 1) **중앙 상단** (v1 우측중앙에서 변경)
- **PauseCountdown** Image: `Resources/Art/UI/Pause/{1,2,3}.png` 3장 sprite, 화면 중앙 (GameUIManager 코드와 함께 salvage)

**v1엔 있지만 Sprint 1에서 제외**:
- `CardBoard` (카드 시스템 영역)
- `StandardCard` (씬 박힌 데모 카드)
- `TouchVisualizer` (touchCircle prefab — 시각 디버그용. 필요 시 Week 2에 추가)
- `NoteTester` (코드 트리거 테스트 도구 — Week 1 step 11 통합 테스트에 유사 패턴 사용 가능)
- `FlickUpZone` / `FlickDownZone` (v1 좌우 플릭 추가 전 잔재. v2는 4방향이라 폐기 또는 zone 4개)

**Week 1 합계 ≈ 29~33h** (Pragmatist 추정 범위)

### Week 2 (목표: 4종 노트 + 점수 + 콤보 동작)

| 작업 | 추정 |
|---|---|
| `InputRouter` 외부 분리 — Touch + Editor 마우스 + 플릭 4방향 각도 | 4h |
| Slide / Flick / Hold 판정 통합 | 4h |
| `ScoreSystem` 분리 — v1 ScoreManager salvage (Plain Mode 분기 이미 있음) | 1h |
| `ComboCounter` 신규 (GameLoop 내부 메서드로 시작) | 1h |
| Lo-Fi 디자인 placeholder UI 배치 (네모상자) | 3h |
| 통합 디버깅 + 채보 동기화 | 4~8h |

**Week 2 합계 ≈ 17~21h**

### Sprint 2 entry 직전 분리 작업 (작동 코드 refactor — 안전)

| 작업 | 비고 |
|---|---|
| `NotePool` 도입 | GC profiler 측정 후 결정 |
| `NoteSpawner` 외부 분리 (GameLoop 내부 메서드 → 클래스) | IDE refactor |
| `NoteDropper` 외부 분리 | IDE refactor |
| `HUDController` 분리 (현재 GameLoop 가 UI 직접 갱신) | IDE refactor |
| `ComboCounter` 외부 분리 | IDE refactor |
| `JudgmentResult` reserve context field 추가 | 카드 시너지 페이로드 대비 |
| C# event 도입 (현재 직접 함수 호출) | publish/subscribe 패턴 |

→ Sprint 2 (카드 시스템) 진입 전 마무리. 작동 코드 기반이라 회귀 위험 낮음.

---

## 4. Sprint 1 placeholder 값 (한울 spec 대기 없이 시작 — 임의 결정)

이재근 결정 (2026-05-16): h 값 등 미확정 spec 은 **임의 placeholder 로 시작**. 나중에 수정.

| 항목 | placeholder 값 | 위치 |
|---|---|---|
| 노트 세로 길이 `h` | **1.0** (Unity world unit) | `GameConfig.NOTE_HEIGHT` |
| 거리 계산 기준점 | 노트 중심 | `Note.GetCenter()` |
| **판정선 y** | **-2.5** (v1 인스펙션 실측. 기존 -2.25 정정) | `GameConfig.JUDGE_LINE_Y` |
| 출발선 y | 7.75 (v1 값) | `GameConfig.SPAWN_Y` |
| BPM | 120 (하드코드 1곡 한정) | `Conductor.bpm` 필드 |
| Offset | 0 | `Conductor.offset` 필드 |
| playerSpeed (r) | 1.0 | `GameConfig.DEFAULT_SPEED` |
| **라인 간격** | **1.0 unit** (v1 21 lane 실측 그대로) 또는 0.875 (24 lane 화면 fit 축소) | `GameConfig.LANE_WIDTH` |
| **카메라** | orthographic, size 10, pos (0, 1.5, -10), bg RGB(49,77,121) | Main Camera |
| **Canvas** | ScreenSpaceCamera, ref res 956×440, Expand, planeDistance 100 | Canvas / CanvasScaler |

향후 갱신 시: 위 const/필드 한 곳만 수정 + 재테스트.

### 4.1 UI placeholder 결정 (2026-05-17)

| 항목 | 결정 | 비고 |
|---|---|---|
| 점수 UI | **v1 참고** — Canvas 좌측중앙 anchor (0, 0.5), anchoredPos (25, 0), TMP. `GameScoreInfo.cs` salvage | v1 인스펙션 확인 |
| 콤보 UI | **중앙 상단** 배치 (v1은 우측중앙이었음 — v2 의식적 변경) | placeholder 네모상자 OK |
| 판정 UI (JudgementText) | 화면 중앙 stretch, fontSize 60 bold, anchoredPos (0, -50), 400px margin | v1 동일 (`LiberationSans SDF`) |
| Pause / Resume UX | **v1 참고** — `GameUIManager.countSprites[3]` = `Resources/Art/UI/Pause/{1,2,3}.png` 카운트다운 sprite salvage | 3-2-1 sprite 시퀀스 |
| HoldNote SpMiss | v1 로직 그대로 (마지막 터치 떼는 시점 SpMiss 등급) | `JudgmentGrade.SpMiss` enum 포함 |
| 플릭 판정 | 거리 조건만 (시간창 X) | 무한 swipe 허용 |

---

## 5. v1 → Sprint 1 매핑 표 (간략)

| v1 → Sprint 1 entry | 비고 |
|---|---|
| `Define.cs` (enum 들) | enum 분리 → `GameConfig` 또는 별 파일 |
| `Utils/NoteType.cs` | `ChartData.cs` — noteType 제거 |
| `Managers/Content/JsonManager.cs` | `ChartLoader.cs` — 2단계 → 단일 |
| `Contents/Note/Note/*.cs` (5개) | `Note.cs` (통합) + `HoldNoteBody.cs` |
| `Contents/Note/NoteScreen.cs` + `Bar.cs` | Week 1: GameLoop 내부 / Week 2: `InputRouter.cs` 분리 |
| `Contents/NoteCreator.cs` | GameLoop 내부 (풀링 X, 직접 Instantiate) |
| `Utils/HitEvent.cs` | `JudgmentResult.cs` (struct, cardEffect 제거) |
| `Managers/Content/ScoreManager.cs` | Week 2: `ScoreSystem.cs` (거의 그대로 salvage) |
| `Contents/GameScoreInfo.cs` | Week 1~2: GameLoop 가 UI 직접 갱신 / Sprint 2 entry: `HUDController.cs` |
| `AudioTest.cs` | `Conductor.cs` 에 흡수 (dspTime 사용) |
| `Contents/GamePlayer.cs` (569줄) | `GameLoop.cs` (250~300줄 목표) |
| `Managers/Managers.cs` | Sprint 1 한정 ScoreManager + JsonManager 만 활성. Card/Deck/Effect 등 빈 stub |
| `Resources/Prefabs/Note/{BasicNote,SlideNote,FlickNote,HoldNote,HoldNoteBody}.prefab` | 동일 경로로 복사 (`.meta` 동반). v2 NoteCreator 인스펙터 ref 재연결 | 5종 그대로 salvage |
| `Resources/Art/UI/Pause/{1,2,3}.png` | 동일 경로로 복사 (`.meta` 동반). Pause 카운트다운 sprite | v1 GameUIManager 패턴 salvage |
| `Resources/Art/barTest.png` | placeholder 단계엔 불필요 (Bar SpriteRenderer enabled=false). Hi-Fi 도착 시 신규 sprite 교체 | 의도적 미salvage |

---

## 6. Sprint 1에서 미도입 (Sprint 2 entry 전 도입)

- **NotePool** — GC profiler 측정 후 도입
- **C# event publish/subscribe** — 직접 함수 호출로 시작
- **HUDController 분리** — GameLoop 가 UI 직접 갱신
- **ComboCounter 외부 분리** — GameLoop 내부 메서드로 시작
- **NoteSpawner / NoteDropper 외부 분리** — GameLoop 내부 메서드로 시작
- **InputRouter 외부 분리** — Week 1 에는 GameLoop 내부, Week 2 에 분리
- **단위 테스트 경계 (POCO 분리)** — Sprint 2 spec 안정 후 도입

---

## 7. Sprint 1 Definition of Done (DoD)

- [ ] 채보 JSON 1개 로드 가능
- [ ] 노트 4종 (Normal/Hold/Slide/Flick) 화면에 떨어짐
- [ ] 4종 노트 모두 판정 가능 (Perfect/Great/Good/Miss)
- [ ] Plain Mode 점수 계산 + 만점 1.2M (만약 모두 Perfect)
- [ ] 콤보 카운터 UI 표시 (네모상자 placeholder OK)
- [ ] 곡 재생 + 채보 동기화 (BPM 120 하드코드 1곡 한정)
- [ ] 일시정지 / 재개 동작

**도달 안 해도 되는 것**:
- 풀링 / 이벤트 시스템 / 외부 분리된 컴포넌트들
- 곡 메타 spec 동적 로드 (하드코드 OK)
- 다중 곡 지원
- Hi-Fi 디자인
- 카드 / 시너지 / 족보 / Challenge Mode

---

## 8. 변경 이력

- 2026-05-16: Council (5 agents, 2 rounds) 결정으로 신설. 14 → 6 컴포넌트 점진 분해. h placeholder 1.0 임의 결정. Sprint 2 entry 분리 작업 명시.
- 2026-05-17: Sprint 1 spec 잔여 빈칸 일괄 확정 (HoldNote SpMiss v1 동일 / r 옵션 X / 플릭 시간창 없음 / 콤보 UI 중앙상단 / 점수·Pause UI v1 참고). Week 1 우선순위 = JSON 채보 드롭 검증 우선, 음원/곡 메타 후순위. §4 UI placeholder 결정 신설.
- 2026-05-17: **v1 Game.unity 인스펙션 결과 통합** — §3.1 Week 1 step 9 (씬 셋업) 구체화 (Camera/Canvas/Bar/UI/Note prefab 5종/Pause sprite 직접 수치 명시). §4 placeholder 표에 카메라/Canvas/라인 간격 실측치 추가. §5 매핑 표에 prefab + sprite salvage 경로 추가. 판정선 y placeholder -2.25 → -2.5 정정.
- 2026-05-17: **Council 5-agent 심의 + Sprint 1 진입 전 9개 spec 확정** — §3.1 씬 루트 8→6 축소 (HUDController + NoteCreator → GameLoop 흡수). Main Camera에 PhysicsRaycaster 부착 명시. Bar 위치 공식 `N - 12.5` 중심정렬 확정 + 콜라이더 9.6 unit 의도 주석. Bar SpriteRenderer Sprint 1 한정 enabled=true + 1×1 white placeholder. URP → Built-in 다운그레이드 (manifest.json). Sprint 1 = "load-bearing spike" 선언.
