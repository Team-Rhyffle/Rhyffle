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

### 🔴 High — Sprint 1 블로커 가능
1. **노트 세로 길이 `h`의 정확값** — ⚠️ **placeholder 1.0 결정 (2026-05-16, 이재근 임의)**, 추후 갱신
   - Unity world units 기준 (전체 4종 노트 동일 적용)
   - v1 참고치: `Set` 시 노트 sprite scale.y = 0.64 unit (정적), Drop 중 = `3.2·cal` (가변)
   - 갱신 위치: `GameConfig.NOTE_HEIGHT`. 한 곳 수정 + 재테스트

2. **거리 계산 기준점** — ⚠️ **노트 중심 결정 (2026-05-16, 이재근 임의)**
   - 노트의 중심점 ↔ 판정선 y 좌표 거리
   - 판정선 위치: world y = `-2.25` (v1 참고치 채택)
   - 갱신 위치: `Note.GetCenter()` 메서드 + `GameConfig.JUDGE_LINE_Y`

3. **HoldNote 사이 판정 노트 자동 생성 룰** ✅ v1 확인 완료
   - **답**: 채보툴이 사전 분할. 같은 `count` 그룹의 여러 JSON 엔트리 = 사이 판정 노트들
   - 게임 클라이언트는 받은 그대로 순차 처리 (v1 `HoldNoteBody.curJudge` 인덱스 진행 방식)
   - 각 엔트리 = 1개 판정 (v1엔 `noteType` 0/1/2 분류, v2엔 제거 — 순서만 position 정렬로 판정)

### 🟡 Medium — 화면 디자인 확정 후 결정
4. **r 적용 범위** — 시스템 기본 r=1. 유저가 r 변경 가능한지? Sprint 1 에 옵션 노출 X (placeholder r=1.0 사용)
5. **라인별 판정선 가로 길이** — 24 lane 중 lane 1개 = world unit 얼마? Figma Hi-Fi blocked
6. **플릭 거리 시간창** — 일정 시간 안에 충족해야 하는지? (예: 노트 도달 후 0.3초 이내)

### 🟢 Low — Plain Mode 외
7. 클래스 점수 / 시너지 점수 공식 placeholder 값들 (Challenge Mode 영역)

### ⚠️ Sprint 1 placeholder 결정 (2026-05-16, 이재근 임의 — 향후 갱신)
- `h` = 1.0 (Unity world unit). 갱신 위치: `GameConfig.NOTE_HEIGHT`
- 거리 계산 기준점 = 노트 중심. 갱신 위치: `Note.GetCenter()`
- 판정선 y = -2.25 (v1 참고치). 갱신 위치: `GameConfig.JUDGE_LINE_Y`
- BPM = 120 (1곡 하드코드), Offset = 0
- playerSpeed (r) = 1.0
- 곡 메타데이터 (BPM/오프셋 형식): 이재근 수정 예정

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
  - `SpMiss` (Special Miss, HoldNote 마지막 터치 떼는 시점) — v2 spec 미언급, 단순 Miss로 통합 권장

### 5.3 v1 코드 참고치 (재구현 시 출발점)
- 노트 sprite scale (Set): `(length·0.16, 0.64, 1)` — 정적 크기
- 노트 sprite scale (Drop): `length·0.8·cal × 3.2·cal × 1` (cal = `height·(-0.08) + 1`) — 원근 효과
- 판정선 위치: world y = `-2.25` (height=0일 때 노트 y)
- 노트 출발 height = 10, 속도 = `speed/480 × 5` unit/sec

## 6. 변경 이력

- 2026-05-16: Notion "게임 시스템 § 3. 판정" 에서 추출, 본 문서 신설. 미확정 7건 트래킹.
