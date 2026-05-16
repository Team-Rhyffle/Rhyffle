# v1 → v2 Salvage Map (Sprint 260514 기준)

> 작성: 2026-05-16
> 범위: Sprint 260514 (플레이 화면 1차 구현 — 채보 로드 + 노트 4종 + 판정 + Plain Mode 점수 + 콤보)
> 제외: 카드 시스템 전부, 시너지, 족보, Challenge Mode

## 요약 (코드 직접 검증 후 갱신)

| 카테고리 | 개수 | 비고 |
|---|---|---|
| 🟢 그대로 salvage | 9 | Plain Mode 분기 이미 있거나 v2 spec과 정합 |
| 🟡 부분 salvage | 7 | 수정/재작성 필요 |
| 🔴 폐기/재작성 | 2 | v2 spec과 충돌 (ReadJudge 판정 + FlickNote 매핑) |
| ⚪ v2 신규 작성 | 4 | v1에 없음 (콤보, 곡 메타, 4방향 플릭 입력, length 의미) |

가장 중요한 조정 4건 (HIGH):
1. **lane 배열 21 → 24** — GamePlayer 전체, HitEvent.cardEffect (선택). v1=7슬롯, v2=8슬롯
2. **판정 시스템 시간 → 거리 기반** — Note.ReadJudge 전면 폐기 + 재작성
3. **FlickNote `direction` 의미 변경** — v1 (0=Down/1=Up) ≠ v2 (0=상/1=우/2=하/3=좌)
4. **HoldNoteInfo `noteType` 필드 제거** — v2 채보 spec에서 이미 빠짐

---

## 시스템별 매핑

### 1. 채보 데이터 컨테이너 & JSON 로드
**등급**: 🟡 부분 salvage

**v1 파일**:
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Utils\NoteType.cs` (데이터 컨테이너)
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Managers\Content\JsonManager.cs` (로더)

**v1 구조**:
- `NoteJson` 루트: 4개 배열 (NormalNotes, HoldNotes, SlideNotes, FlickNotes)
- `BasicNoteInfo`, `SlideNoteInfo`, `FlickNoteInfo`: `position`, `line`, `length`
- `HoldNoteInfo`: `position`, `line`, **`noteType`**, `count`, `length`
- JsonManager: Newtonsoft.Json + Resources.Load 사용

**v2 적용 시 주의**:
1. **HoldNoteInfo.noteType 제거** (HIGH) — 구조체 정의에서 필드 삭제, 역직렬화 path 정리
2. **line 인덱싱**: v1/v2 모두 정수, v2는 [0,23] — 호환 OK
3. **FlickNote.direction 의미 변화** (HIGH) — v1 0/1만 / v2 0/1/2/3 (시계방향 상/우/하/좌)

### 2. 노트 기본 클래스
**등급**: 🟡 부분 salvage

**v1 파일**: `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\Note\Note\`
- `Note.cs`, `BasicNote.cs`, `SlideNote.cs`, `FlickNote.cs`

**v1 구조** (확인 완료):
- `Note.SetJudge(int judge)` — judge × 6.4f 변환 (120 BPM 가정 상수)
- `Note.Drop(float speed)` — height 감소, 원근 효과 `cal = height·(-0.08) + 1`
  - scale = `(length·0.8·cal, 3.2·cal, 1)`, pos = `(lanePos·cal, height - 2.25, 2)`
  - 판정선 = world y `-2.25`
- `Note.Set(lane, length)`: scale=`(length·0.16, 0.64, 1)`, posY=`7.75` (출발선)
- `Note.ReadJudge(...)`: **시간 기반** — `bpm/600·k·16` (k = 1, 1.5, 2)

**v2 적용 시 주의**:
1. **판정 방식 자체가 다름** (HIGH) — v1 시간 기반 → v2 거리 기반 `(1+k·r)·h`. **ReadJudge 완전 폐기 + 재작성**
2. **`SetJudge` 6.4f 상수** (MEDIUM) — 120 BPM 가정, BPM 동적화
3. **`Drop` 공식 상수** (MEDIUM) — Figma Hi-Fi 확정 후 조정. v1의 0.08/2.25/0.8/0.64 참고치로만
4. **FlickNote `flicDir` ↔ `direction` 매핑** (HIGH) — v1 (0=Down/1=Up) vs v2 (0=상/1=우/2=하/3=좌). 채보 데이터 호환 X — v2는 새 채보 사용
5. **`JudgementType.SpMiss`** (LOW) — v1 HoldNote 마지막 endtouch miss 전용. v2 spec엔 없음 — 단순 Miss로 통합

**재활용 전략**:
- ✅ Set/Drop 메서드 골격 유지 (좌표 변환 + 원근 효과)
- 🔴 ReadJudge 전체 폐기. 거리 기반으로 신규 작성
- ⚠️ Drop 상수: Hi-Fi 디자인 확정 후 재유도

### 3. 홀드 노트 (HoldNote, HoldNoteBody)
**등급**: 🟡 부분 salvage

**v1 파일**:
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\Note\Note\HoldNoteBody.cs`
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\Note\Note\HoldNote.cs`

**v1 구조** (확인 완료):
- `HoldNote`: Note 상속, 본체 로직은 부모 그대로
- `HoldNoteBody`: 홀드 폴리라인 컨테이너
  - `holdNotes: List<HoldNote>` — **사이 판정 노트들** (채보툴이 사전 분할)
  - `curJudge: int` — 진행 인덱스
  - `LineRenderer + widthCurve` 로 폴리라인 (`InverseLerp(minY, maxY)` 로 normalize, scale = `height·(-0.08)+1`)
  - ReadJudge: curJudge==0(첫, press) → 중간(intouch, 자동 시간 판정) → 마지막(endtouch)

**v2 적용**:
- ✅ **폴리라인 그룹핑 구조 정합** — JSON `count` 그룹 = 사이 판정 노트들, v1 처리 방식과 동일
- 🔴 판정 윈도우 (시간 → 거리) v2 공식으로 재구현
- ⚠️ `JudgementType.SpMiss` 사용 부분 — v2엔 없음, 단순 Miss로 변경
- ⚠️ `DrawLine`: Figma Hi-Fi에 맞춰 시각 조정. widthCurve 공식 (`scale·1.5`) 재검토

### 4. 노트 생성 (NoteCreator.cs)
**등급**: 🟢 그대로 salvage

**v1 파일**: `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\NoteCreator.cs`

**v1 구조**: `CreateBasic/Slide/Flick/HoldBody()` — Instantiate → Set → SetJudge

**v2 적용**: prefab 참조와 경로만 v2에 맞게 교체. 클래스 본체는 그대로.

### 5. 입력 처리 (NoteScreen, Bar)
**등급**: 🟡 부분 salvage

**v1 파일**:
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\Note\NoteScreen.cs`
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\Note\Bar.cs`

**v1 구조**: 다중 터치(5개) + RaycastHit로 Bar 감지. 플릭 = angle 계산 (30~150 = up, -30~-150 = down). GamePlayer.press[]/slide[]/intouch[]/endtouch[]/flickUp[]/flickDown[] 갱신.

**v2 적용 시 주의**:
1. **플릭 4방향 확장** (HIGH) — 각도 범위 4구간으로 재계산 (예: 상 -45~45, 우 45~135, ...)
2. `flickUp[]`/`flickDown[]` 배열 → `flickByDir[4][]` 로 일반화 권장
3. lane 범위 [0,23] 호환

### 6. 판정 + 점수
**등급**: 🟢/🟡 — ScoreManager 🟢, GameScoreInfo + HitEvent 🟡

**v1 파일**:
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Managers\Content\ScoreManager.cs` — 🟢
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\GameScoreInfo.cs` — 🟡
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Utils\HitEvent.cs` — 🟡

**v1 ScoreManager 구조** (확인 완료):
- `Init(totalNoteCount)`: baseScorePerNote = round(1M/count), remainderFirstNote 계산
- `ApplyNoteScore(first, judgement, rank, scale)`: 점수 적용
- `GetJudgementMultiplier`: Perfect 1.2 / Great 1.0 / Good 0.7 / Miss 0
- **`CurrentMode = Plain/Challenge` 분기 이미 존재** — Plain Mode일 때 cardBonus=0, multiplier=1
- `Managers.Effect.Brands` / `.Effects` null check 자동 skip (Plain Mode 안전)
- → **🟢 그대로 salvage. 카드 코드 제거 불필요.** v2 Plain Mode spec 완벽 일치

**v1 GameScoreInfo 구조** (확인 완료):
- `handRankScore[13]` (포커 족보별 배율) — Challenge Mode 전용
- `NormalScoring()` 메서드 — **현재 비어있음** (Plain Mode용 placeholder)
- `CardScoring(hitEvents)` — Challenge Mode용, 내부에 `Managers.Card.FieldCards`, `Managers.Hand.Evaluate` 호출
- → **🟡 부분 salvage**:
  - 옵션 A: `NormalScoring(hitEvents)` 구현 후 GamePlayer에서 분기 호출
  - 옵션 B: GameScoreInfo 우회, GamePlayer에서 직접 `Managers.Score.ApplyNoteScore` 호출
  - 현재 v1은 Plain Mode에서도 CardScoring 호출 중 (이름 미스리딩) — 정리 필요

**v1 HitEvent 구조** (확인 완료):
- `judge`, `handRank`, `minRange`, `maxRange`, `scale`, `rank`, `cardEffect[7]`
- cardEffect 자동 계산 (minRange/maxRange 기반)
- → **🟡 부분 salvage**: cardEffect 배열 크기 7 → 8 변경 권장 (8 슬롯 정합). Plain Mode에선 dead state로 무관

### 7. 콤보 카운터
**등급**: ⚪ v2 신규 작성

v1에 명시적 콤보 로직 미발견. Sprint 1 요구사항이라 신규:
- `combo: int`
- Perfect/Great/Good → +1, Miss → 0
- UI 갱신 이벤트

### 8. 곡 재생 & 시간 관리
**등급**: 🟡 부분 salvage

**v1 파일**:
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\AudioTest.cs`
- `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\GamePlayer.cs`

**v1 구조**:
- AudioTest: 2초 지연 후 Play, Pause/Resume
- GamePlayer:
  - `currentTime += Time.deltaTime * (bpm/60) * 16`
  - `bpm = 192` (120 BPM의 16배 → step 단위 변환)
  - `chartToolOffset = 6.4f` (position step → 게임 시간 변환 상수)
  - `offSet = 0` (미사용)

**v2 적용 시 주의**:
1. **BPM 하드코드 → 동적** (HIGH) — 곡 메타데이터에서 주입. (메타 spec 수정 예정)
2. **chartToolOffset 6.4f** (MEDIUM) — BPM 변화 시 공식 재검증
3. **곡 오프셋** (MEDIUM) — 메타에서 로드 후 적용
4. **2초 지연** (LOW) — 필요성 검증, offset으로 대체 가능

### 9. 게임 루프 (GamePlayer.cs)
**등급**: 🟡 부분 salvage (수정 다수)

**v1 구조** (확인 완료):
- **UniTask 사용** (`Cysharp.Threading.Tasks`) — Update 대신 `async UniTask GameSystem()` 무한 루프 + `await UniTask.WaitUntil(() => play)` 일시정지 처리
- 21개 lane 배열: `press/slide/intouch/endtouch/flickUp/flickDown/touchStart/touchEnd/touchCon/judgeChecker/secondJudgeChecker` 모두 [21]
- `bpm = 120 / 5 * 8 = 192` — 시간 증가 계수 (실제 곡 BPM과 다름)
- `playerSpeed = 4` (기본 배속)
- `currentTime = -(bpm/60) * 32` 시작 (음수 카운트다운)
- `currentTime += Time.deltaTime * (bpm/60) * 16` 매 프레임 증가
- `chartToolOffset = 6.4f`
- **노트 스폰 시점**: `currentTime > position·6.4 - 64·4/playerSpeed` (≈ 1박자 일찍)
- 판정 루프: 21 lane × inGameNote 순회 → checkType 1~6 순차 적용 → HitEvent 생성 → `scoreInfo.CardScoring(hitEvents)` → 매 프레임 reset
- **풀링 미구현** ("Need Pooling" 주석 다수, `Destroy(temp)` 직접 호출)
- `cardChangeTimer = 20` (20초마다 카드 교환)
- `using NUnit.Framework;` 미사용 import

**v2 적용**:
- 🔴 **lane 배열 21 → 24** — 모든 bool[]/int[] 크기 변경
- 🔴 **ReadJudge 호출 인자 변경** — `bpm` 빠지고 `h`, `r` (또는 직접 거리값) 들어감
- ❌ `cardChangeTimer`, `Managers.Card.ResetCards()`, `Managers.Deck.DoNothing()` 제거
- ⚠️ `scoreInfo.CardScoring` → Plain Mode용 분기 호출 (또는 직접 `Managers.Score.ApplyNoteScore` 호출)
- ⚠️ `bpm` 변수: 곡별 동적 (메타 spec 도착 후)
- ⚠️ `chartToolOffset 6.4f`: BPM 동적화 후 공식 재유도
- ⚠️ `SpMiss` 처리: v2엔 없음 — Miss로 통합
- ✅ UniTask 루프 구조 유지 (또는 Update로 단순화 선택)
- ✅ 노트 스폰 + Drop + 판정 + reset 골격 유지
- ⚠️ 풀링 도입 검토 (Sprint 1엔 그대로도 OK)
- ✅ 미사용 import 정리

### 10. 매니저 (Managers.cs)
**등급**: 🟡 부분 salvage

**v1 구조** (확인 완료):
- Singleton — `Managers.Card/Deck/Effect/UI/Hand/MainGame/Score/Json` (Content) + `Data/Input/Pool/Resource/Scene` (Core)
- `@Managers` 프리팹 (`Resources.Load<GameObject>("Prefabs/@Managers")`) 의존
- `DontDestroyOnLoad` 적용
- Init: card/deck/effect/pool/scene 초기화 (Json/Score는 Init 미호출 — lazy)
- Update: `_card.OnUpdate()`, `_input.OnUpdate()`

**v2 적용**:
- ✅ Singleton 구조 + JsonManager/ScoreManager 그대로
- ⚠️ Card/Deck/Effect/Hand/MainGame 매니저: Sprint 1엔 미사용. **빈 stub 유지** 또는 호출 회피 (`Managers.Effect.Brands` 같은 null check는 ScoreManager에 이미 있음)
- ⚠️ `@Managers` 프리팹 v2에서 신규 작성 필요

### 11. 입력 라인 (Bar.cs)
**등급**: 🟢 그대로 salvage

**v1 구조** (확인 완료):
- `barNum` per lane, `gamePlayer` 참조
- **`#if UNITY_EDITOR`** 가드 — Editor 마우스 이벤트만 (모바일은 NoteScreen이 처리)
- `OnMouseDown/Enter/Over/Drag` → `gamePlayer.press/slide/intouch/endtouch/flickUp/flickDown[barNum]` 갱신

**v2 적용**: 그대로. v2 lane 24개라 24개 Bar GameObject 필요.

---

## 의존성 그래프

```
GamePlayer
├── NoteCreator
│   └── BasicNote / SlideNote / FlickNote / HoldNoteBody (→ Note)
├── NoteScreen (입력) → Bar
├── GameScoreInfo (점수)
│   ├── HitEvent
│   └── ScoreManager
├── AudioTest (음악)
├── Managers
│   ├── JsonManager (채보 로드)
│   └── ScoreManager (점수 누적)
└── Define (enum)
```

---

## 카드 시스템 (Sprint 1 제외, 추후 참고용)

**v1 위치**: `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle\Assets\Scripts\Contents\Card\`

**주요 클래스**:
- `CardBase.cs` — 기본 클래스, `OnNoteTrigger` 메서드
- `DiamondFieldManager.cs` — 족보 판정 (야구 필드 메타포)
- `War Of The Roses/` — 80+ 카드 (검은/빨간 장미)
- `DeckBuilding/` — 덱 빌드 UI

**v1 메모 (`Assets/필요 로직 목록.md`)**:
1. `presetJudgementType` 기반 판정 강제 (예: 모든 판정 miss)
2. 족보 점수 배율 +/× 연산 확장

→ **Sprint 1엔 가져오지 말 것**. Sprint 2+ 카드 도입 시 별도 매핑 작성.

---

## 알려진 이슈 / 주의사항

| 우선순위 | 항목 | v1 상태 | v2 요구 | 조치 |
|---|---|---|---|---|
| HIGH | HoldNote.noteType 제거 | 존재 | 미포함 | 구조체 재정의 |
| HIGH | Flick direction 4방향 | 0/1만 | 0/1/2/3 (시계방향) | 매핑 + 입력 각도 재구간 |
| HIGH | 판정 윈도우 spec | bpm/600*16식 | (1+k·r)*h | Notion 확인 후 재구현 |
| MEDIUM | BPM 동기화 | 하드코드 120 | 곡별 동적 | 메타에서 주입 (spec 수정 예정) |
| MEDIUM | Drop 상수 (0.08/2.25/0.8) | 고정값 | UI 따라 변경 가능 | Figma Lo-Fi 기준 확정 |
| MEDIUM | chartToolOffset 6.4f | 120 BPM 가정 | BPM 의존 | 공식 재유도 |
| LOW | 2초 지연 (AudioTest) | 하드코드 | 필요성 확인 | offset으로 대체 가능 |
| LOW | 카드 매니저 stub | 의존 있음 | Sprint 1 미사용 | 제거 또는 빈 stub |

---

## 권장 Salvage 순서 (의존성)

1. `Define.cs` (enum)
2. `Utils/NoteType.cs` — **noteType 필드 제거**하고 가져옴
3. `Managers/Content/JsonManager.cs`
4. `Contents/Note/Note/Note.cs` → BasicNote/Slide/Flick (판정 공식 v2로 교체)
5. `Contents/Note/Note/HoldNoteBody.cs` (판정 공식 v2)
6. `Contents/NoteCreator.cs` (그대로)
7. `Contents/Note/NoteScreen.cs` + `Bar.cs` (플릭 4방향 확장)
8. `Utils/HitEvent.cs` (cardEffect 제거)
9. `Managers/Content/ScoreManager.cs` (카드 로직 제거)
10. `Contents/GameScoreInfo.cs` (CardScoring → ApplyScoring)
11. `AudioTest.cs`
12. `Contents/GamePlayer.cs` (카드 참조 제거)
13. `Managers.cs` (Card/Deck/Effect 매니저 stub or 제거)
14. **신규**: ComboCounter

---

## 수정 규모 추정

| 시스템 | 파일 수 | 핵심 변경 | 난이도 |
|---|---|---|---|
| 채보 파서 | 2 | noteType 제거 | 낮음 |
| 노트 클래스 | 6 | 판정 윈도우 + flick 4방향 | 중간 |
| 입력 처리 | 2 | flick 각도 4구간 | 중간 |
| 점수 시스템 | 3 | 카드 로직 제거 | 낮음 |
| 시간 관리 | 2 | BPM 동적화 | 중간 |
| 게임 루프 | 1 | 카드 참조 제거 | 중간 |
| 매니저 | 1 | 정리 | 낮음 |
| 콤보 (신규) | 1 | 신규 | 낮음 |
| **합계** | **18** | — | **중간** |

---

## 다음 액션 후보

1. 위 권장 순서대로 1번부터 salvage 시작 (Define + NoteType + JsonManager 셋이 의존성 root)
2. 그 전에 v2 판정 윈도우 spec을 Notion에서 정확히 확인 (현 spec 표현 `(1 + k·r) * h` — k가 단계별로 0.2/0.4/0.7/1.0 인지 등)
3. 또는 곡 메타 spec 도착 후 BPM/오프셋 부분 한 번에 처리
