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
| 9 | Unity 씬 셋업 (24 Bar 배치 스크립트, 카메라, 판정선) | 4h |
| 10 | 첫 채보 더미 데이터 (Tap 위주, 모든 노트 종류 1개씩) — 직접 작성 or 김민주 요청 | 2h |
| 11 | 통합 테스트 (노트 떨어짐 + Tap 판정) | 4~8h |

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
| 판정선 y | -2.25 (v1 값) | `GameConfig.JUDGE_LINE_Y` |
| 출발선 y | 7.75 (v1 값) | `GameConfig.SPAWN_Y` |
| BPM | 120 (하드코드 1곡 한정) | `Conductor.bpm` 필드 |
| Offset | 0 | `Conductor.offset` 필드 |
| playerSpeed (r) | 1.0 | `GameConfig.DEFAULT_SPEED` |

향후 갱신 시: 위 const/필드 한 곳만 수정 + 재테스트.

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
