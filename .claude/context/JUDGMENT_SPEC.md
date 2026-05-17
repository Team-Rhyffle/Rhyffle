# Rhyffle v2 — 판정 spec (정리)

> 정본: Notion "게임 시스템 § 3. 판정" — https://www.notion.so/362a95ab77ed818ba887c5373786b1c8
> 이 문서는 implementation 빠른 참조용 + 미확정 항목 트래킹.

## 1. 공통 판정 윈도우 — 거리 기반

**변수**:
- `h` = 노트 오브젝트의 **세로 길이** (Unity world units 추정)
- `r` = **배속** (system 기본 r=1, 지원 범위 [0.1, 3.0])

**4단계 판정 임계값** (절대값 기준, 거리 ≤ 임계 시 해당 등급):

| 등급 | 거리 임계 | 점수 배율 | 색상 |
|---|---|---|---|
| **Perfect** | `(1 + 0.2·r) · h` | **120%** | 하늘색 |
| **Great** | `(1 + 0.4·r) · h` | **100%** | 연두색 |
| **Good** | `(1 + 0.7·r) · h` | **70%** | 주황색 |
| **Miss** | `(1 + 1.0·r) · h` | **0%** (놓침) | — |

→ k 계수: 0.2 / 0.4 / 0.7 / 1.0

### 해석 노트
- **거리 기반** 판정 (시간 기반 아님). 터치 순간 노트와 판정선의 (월드/스크린) 거리로 판정
- 배속 r 증가 시 임계도 증가 — 빠른 속도 보정 (시간으로 환산하면 동일 시간 윈도우)
- Miss 임계 = `(1 + r) · h` 를 넘기면 "노트 놓침" 처리

## 2. 노트 타입별 세부 판정

### NormalNote (탭)
- 위 4단계 그대로 적용

### HoldNote
- **사이 판정 노트** 단위로 별개 판정 (긴 hold 한정)
  - 짧은 hold = 시작/끝 2회 판정
  - 긴 hold = BPM 따라 사이 판정 노트 추가됨 (자동 분할)
- 8개 사이 판정 노트 중 4개 판정 후 끊김 → 5번째부터 **모두 fail 처리**
- **노트 개수 카운트**: hold 1개가 아니라 사이 판정 노트 수만큼 카운트 (점수/콤보 분모)
- **콤보**: 각 사이 판정 노트마다 개별 +1 (또는 끊김 시 reset)

### SlideNote
- 누를 필요 없이 **터치 상태 유지**되면 처리
- 판정 등급은 NormalNote와 동일 기준

### FlickNote
- **2단계 조건**:
  1. **최소 조건**: 지정 거리만큼 해당 방향 스와이프 — 충족 못 하면 noflicked 처리 (=Miss?)
  2. **충족 시**: 탭 노트와 동일 기준으로 Perfect/Great/Good 판정
- **스와이프 거리 임계**: 라인별 판정선 가로 길이의 **1/3**
  - 상하플릭 = y축 거리 ≥ (라인 가로폭) × 1/3
  - 좌우플릭 = x축 거리 ≥ (라인 가로폭) × 1/3

## 3. 점수 계산 (Plain Mode — Sprint 1 한정)

```
baseScore     = 1,000,000 / totalNoteCount    (소수점 첫째자리 반올림)
remainder     = 1,000,000 - baseScore × (totalNoteCount - 1)
                → 첫 노트 점수에 합산 (만점 보정용)

per-note      = baseScore × judgmentMultiplier
                  Perfect 1.2 / Great 1.0 / Good 0.7 / Miss 0

totalScore    = Σ per-note    (만점 = 1,200,000)
```

- **HoldNote 카운트**: 사이 판정 노트 수만큼 분모에 포함 (Notion 명시)
- Challenge Mode (카드/시너지/클래스 점수) → **Sprint 1 제외**

## 4. 미확정 항목 (implementation 진입 전 결정 필요)

### ✅ 해소 (2026-05-17, 이재근 결정 — 본인 권한 내)
1. **노트 세로 길이 `h`** — 1.0 유지 (Unity world unit). 갱신 위치: `GameConfig.NOTE_HEIGHT`
2. **거리 계산 기준점** — 노트 중심. 판정선 y = -2.5. 갱신 위치: `Note.GetCenter()` / `GameConfig.JUDGE_LINE_Y`
3. **HoldNote 사이 판정 노트 자동 생성 룰** — 채보툴이 사전 분할. 같은 `count` 그룹의 여러 JSON 엔트리 = 사이 판정 노트들. 게임 클라는 position 정렬 후 순차 처리
4. **r 적용 범위** — Sprint 1에 옵션 노출 X. `GameConfig.DEFAULT_SPEED = 1.0` 하드코드
5. **라인별 판정선 가로 길이** — **임의 결정 — 화면 가로에 맞게**. 24 lane 전체가 카메라 가시 영역 가로 폭을 채우도록 셋업 시 산출. v1 참고치(`length·0.16`)와 별개로 v2는 화면 fit 우선. 갱신 위치: `GameConfig.LANE_WIDTH`
6. **플릭 거리 시간창** — **없음**. 거리 조건만 충족하면 판정. 시간 제한 도입 X
7. **HoldNote SpMiss** — **v1과 완벽히 동일한 로직**. (v1: 마지막 터치를 떼는 시점에 등급 분류, SpMiss enum 유지). v2 `JudgmentGrade` enum에 `SpMiss` 포함 + v1 `HoldNoteBody` salvage 시 로직 그대로

### 🟢 Low — Plain Mode 외 (Sprint 1 범위 외)
- 클래스 점수 / 시너지 점수 공식 placeholder 값들 (Challenge Mode 영역)

### ⚠️ Sprint 1 placeholder 결정 (2026-05-16 임의 → 2026-05-17 한울 권한 위임 확정 + v1 인스펙션 실측 반영)
- `h` = 1.0 (Unity world unit). 갱신 위치: `GameConfig.NOTE_HEIGHT`
- 거리 계산 기준점 = 노트 중심. 갱신 위치: `Note.GetCenter()`
- **판정선 y = -2.5** (v1 Bar world y 실측. 기존 -2.25 placeholder 정정). 갱신 위치: `GameConfig.JUDGE_LINE_Y`
- BPM = 120 (1곡 하드코드), Offset = 0
- playerSpeed (r) = 1.0
- **라인 간격 = 1.0 unit** (v1 21 lane Bar 간격 실측, v2 24 lane은 그대로 1.0 또는 화면 fit 0.875 비례 축소 중 선택). 갱신 위치: `GameConfig.LANE_WIDTH`
- **카메라 = orthographic, size 10, pos (0, 1.5, -10)** (v1 동일)
- **Canvas = ScreenSpaceCamera, ref res 956×440, ScaleWithScreenSize/Expand** (v1 동일)
- 곡 메타데이터 (BPM/오프셋 형식): 이재근 수정 예정 (음원/곡 메타는 JSON 검증 끝나고 후순위)
- 참조: `V1_SCENE_INSPECTION.md` (v1 실측 전체)

## 5. v1 코드와의 차이 (확인 완료)

### 5.1 판정 방식 자체가 다름
| 항목 | v1 | v2 (Notion spec) |
|---|---|---|
| 판정 기준 | **시간 기반** (`curTime` vs `judge`) | **거리 기반** (노트 ↔ 판정선) |
| 윈도우 변수 | BPM 의존 (`bpm/600·k·16`) | 노트 길이 `h` + 배속 `r` |
| Perfect | `±bpm/600·16` | `(1 + 0.2·r)·h` |
| Great | `±bpm/600·1.5·16` | `(1 + 0.4·r)·h` |
| Good | `±bpm/600·2·16` | `(1 + 0.7·r)·h` |
| Miss | 그 외 | `(1 + 1.0·r)·h` 초과 |

→ **v1 ReadJudge 전면 폐기 + 거리 기반 신규 구현**

### 5.2 노트 타입별 추가 차이
- **FlickNote**: v1 `flicDir` 0=Down/1=Up (2방향) vs v2 `direction` 0=상/1=우/2=하/3=좌 (4방향, 시계방향)
  - v1 채보 데이터와 v2 호환 X. v2는 새 채보 사용
  - 매핑: v1 0(Down) ↔ v2 2(Down) / v1 1(Up) ↔ v2 0(Up)
- **HoldNote**:
  - v1 `noteType` 필드 (0:시작/1:중간/2:끝) — v2 제거됨
  - 사이 판정 노트는 v1/v2 모두 채보툴이 사전 분할 (정합 ✓)
  - 폴리라인 처리 로직 (`HoldNoteBody.curJudge` 진행) — v2도 유사하게 사용 가능
- **JudgementType enum**: v1 `Perfect, Great, Good, Miss, SpMiss, Checked, NotChecked`
  - `SpMiss` (Special Miss, HoldNote 마지막 터치 떼는 시점) — **v2도 유지 (2026-05-17 이재근 결정). v1 로직 그대로 salvage**

### 5.3 v1 코드 참고치 (재구현 시 출발점)
- 노트 sprite scale (Set): `(length·0.16, 0.64, 1)` — 정적 크기
- 노트 sprite scale (Drop): `length·0.8·cal × 3.2·cal × 1` (cal = `height·(-0.08) + 1`) — 원근 효과
- 판정선 위치: world y = `-2.25` (height=0일 때 노트 y)
- 노트 출발 height = 10, 속도 = `speed/480 × 5` unit/sec

## 6. 변경 이력

- 2026-05-16: Notion "게임 시스템 § 3. 판정" 에서 추출, 본 문서 신설. 미확정 7건 트래킹.
- 2026-05-17: 미확정 6건 전원 해소 (이재근 본인 권한 결정 + 한울 권한 위임). h=1.0 유지, r 옵션 X, 라인 가로폭 화면 fit, 플릭 시간창 없음, HoldNote SpMiss v1 로직 그대로 유지. Plain Mode 외 클래스/시너지만 잔존.
