# Rhyffle v2 — 채보 파일 포맷 spec

> 채보툴(김은규 개발) 출력 JSON 포맷. Sprint 1 채보 파서의 입력 spec.
> 출처: 김은규 작성 `NoteJson.cs` (v1 repo, 게임 클라이언트의 채보 데이터 컨테이너)
> 샘플: `.claude/context/samples/chart_sample.json`

## 1. 최상위 구조

```jsonc
{
  "NormalNotes": [ NormalNoteData, ... ],
  "HoldNotes":   [ HoldNoteData, ... ],
  "SlideNotes":  [ SlideNoteData, ... ],
  "FlickNotes":  [ FlickNoteData, ... ]
}
```

노트 타입별로 4개의 별도 리스트. 채보툴이 타입별로 정렬해서 출력.

## 2. 타입별 스키마

| 타입 | 필드 | 채보툴 키 | 게임 내 매핑 |
|---|---|---|---|
| NormalNote | `position`, `line`, `length` | **Z** (= "BasicNote") | 탭 노트 |
| HoldNote | `position`, `line`, `count`, `length` | **V** | 홀드 노트 (count로 묶음) |
| SlideNote | `position`, `line`, `length` | **X** | 슬라이드 노트 |
| FlickNote | `position`, `line`, `length`, `direction` | **C** | 플릭 노트 (4방향) |

⚠️ 명칭 주의: 채보툴 UI는 "BasicNote", 데이터 스키마는 "NormalNote" — **동일 노트**.

## 3. 필드 의미

### 공통 필드

| 필드 | 타입 | 의미 | 단위 |
|---|---|---|---|
| `position` | int | 노트 등장 시점 | 정수 step. **3 = 1/64박자** 기준 (48 = 1/4박자) |
| `line` | int | 노트의 **가장 왼쪽** 레인 (lane) | **0-23** 범위 (0-indexed, 8 슬롯 × 3 세분화) |
| `length` | int | 노트의 **가로 폭** (덮는 lane 개수) | 최소 1. line=4, length=7 → lane 4~10 차지. **length=0 미사용** |

### HoldNote 전용

| 필드 | 타입 | 의미 |
|---|---|---|
| `count` | int | 홀드 체인 그룹 ID. **같은 count = 하나의 홀드** |

- 한 홀드 = 여러 키포인트 (`HoldNoteData` 엔트리들)로 구성
- 게임 클라가 같은 `count`끼리 묶고 `position` 오름차순 정렬 → 폴리라인 형태로 시각화
- 각 키포인트의 `(line, length)` 가 그 시점의 홀드 띠 위치+폭 정의 (홀드가 lane을 가로질러 움직이거나 폭이 변할 수 있음)
- `noteType` 필드는 **제거됨** (2026-05-09 김은규 제안 → 2026-05-16 반영 확정)

### FlickNote 전용

| 필드 | 타입 | 의미 |
|---|---|---|
| `direction` | int | 플릭 방향. 0/1/2/3 — 4방향, **시계방향 순서** |

- 매핑: **0 = 상(↑), 1 = 우(→), 2 = 하(↓), 3 = 좌(←)** (12시 시작 시계방향)
- 채보툴에서 W 키 회전으로 입력
- "파란색이 있는 방향으로 플릭" (채보툴 사용법 명시)

## 4. 시간 단위 표

채보툴 F1~F9 박자 키 대응표:

| 박자 | step 수 | 채보툴 키 |
|---|---|---|
| 1/64 | 3 | F5 |
| 1/32 | 6 | F4 |
| 1/24 | 8 | F9 |
| 1/16 | 12 | F3 |
| 1/12 | 16 | F8 |
| 1/8 | 24 | F2 |
| 1/6 | 32 | F7 |
| 1/4 | 48 | F1 |
| 1/3 | 64 | F6 |

- **공식**: `step = 192 / (4 × 분모)` (4박자 1마디 = 192 step). 즉 한 마디 = 192 step, 1/64 음표 = 3 step
- 예: 한 마디 4박자 = 192 step / position 0, 48, 96, 144 = 박자 1, 2, 3, 4

## 5. 레인 구조

- **총 24개 레인** (8 슬롯 × 3 세분화) — 게임 시스템 spec과 정합
- **0-indexed**: `line` ∈ [0, 23]
- `length` = 노트가 덮는 lane 수 (≥ 1)
- **제약**: `line + length ≤ 24` (마지막 노트 lane = `line + length - 1` ≤ 23)

## 6. BPM / 곡 메타데이터

- 채보툴에 BPM 별도 입력 (UI 박스 + Enter)
- 현재 JSON에 BPM/오프셋 필드 미포함
- ⏳ **곡 메타데이터 spec 수정 예정** (이재근 2026-05-16. 아직 변경 미반영 상태)
- 수정 도착 시 본 문서 갱신

## 7. 게임 클라 (Sprint 1) 파서 가이드

### 로드 순서
1. JSON 파싱 → 4개 리스트 획득
2. **HoldNote**: `count`별 그룹핑 → 각 그룹 `position` 오름차순 정렬 → 폴리라인 데이터 구축
3. 나머지는 그대로 사용

### 렌더링
- 각 노트의 `position`을 곡 시간으로 변환 (BPM × step → seconds)
- 24 lane 좌표계에서 `line`부터 `length` 만큼 가로 폭으로 그리기
- HoldNote: 키포인트 간 보간 (폴리라인) — `(time, line_left, line_right)` 형태 삼각망 또는 사다리꼴 메시

### 판정
- 노트 도착 시점 = position 변환된 곡 시간
- 판정 윈도우 = Notion "게임 시스템 3. 판정" 공식 적용 (Perfect/Great/Good/Miss)
- HoldNote: 키포인트별 판정 (Notion spec — "8개의 판정 노트로 이어진 홀드노트 중 4개 받고 끊기면 5번째부터 fail")
  - 각 HoldNote 엔트리 = 1개 사이판정 노트로 취급

## 8. C# 데이터 컨테이너 (참고 — v1 코드)

`NoteJson.cs` (김은규 작성):
```csharp
[Serializable] public class ChartData {
    public List<NormalNoteData> NormalNotes;
    public List<HoldNoteData> HoldNotes;
    public List<SlideNoteData> SlideNotes;
    public List<FlickNoteData> FlickNotes;
}

[Serializable] public struct NormalNoteData { int position, line, length; }
[Serializable] public struct HoldNoteData   { int position, line, count, length; }
[Serializable] public struct SlideNoteData  { int position, line, length; }
[Serializable] public struct FlickNoteData  { int position, line, length, direction; }
```

- Unity `JsonUtility` 로 직접 deserialize 가능
- v2 salvage 시: `MonoBehaviour` 상속 제거 (필드 없는 컨테이너이므로 일반 class면 됨), `using NUnit.Framework;` 미사용 import 제거

## 9. 미확정 항목

✅ 해소됨 (2026-05-16):
- FlickNote `direction` 매핑 → 0=상/1=우/2=하/3=좌 (시계방향)
- line 인덱싱 → 0-indexed (0~23)
- `length=0` → 미사용 케이스

⏳ 진행 중:
- 곡 메타데이터 (BPM/오프셋 등) — 수정 예정

## 10. 변경 이력

- 2026-05-03: 박자 단위 정의 (정수 step, 3=1/64, 48=1/4) — 김은규+이재근
- 2026-05-09: HoldNote `noteType` 제거 제안 — 김은규
- 2026-05-11: 채보툴 v1 첫 빌드 공유 — 김은규
- 2026-05-16: HoldNote 스키마 변경 반영 확정 — 이재근. 본 spec 문서 신설
- 2026-05-16: 미확정 항목 3건 해소 — 이재근 (direction 시계방향 매핑, line 0-indexed, length=0 미사용). 곡 메타 수정 예정 표기
