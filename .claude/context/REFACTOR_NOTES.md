# v1 → v2 리팩터링 노트

> v1 코드 점검에서 발견된 코드 품질 이슈. salvage 시 함께 정리.
> 카테고리: 🐛 버그 가능성 / 🎯 매직 넘버·네이밍 / 🏗 구조 / ⚡ 성능 / 🧹 dead code

---

## 🐛 잠재 버그 / 의심스러운 로직

### B1. `GameScoreInfo.ChangeAllRankBonus` — `=` 인데 `+=` 의도
```csharp
public void ChangeAllRankBonus(float allRank)
{
    allRankBonus = + allRank;  // = 이므로 누적 안 됨. + 는 unary +
}
```
→ 의도가 누적이면 `+=`, 단순 설정이면 `+` 단항 제거. **확인 필요**

### B2. `SlideNote.ReadJudge` checkType==2 분기 — 조건 부등호 혼재
```csharp
if(curTime < judge - bpm/600*16)
{
    return JudgementType.Checked;
}else if(curTime > judge - bpm/600*16)  // ← 위 조건 부정
{
    return JudgementType.Perfect;
}else if(curTime > judge - bpm/600*1.5f*16)  // ← 항상 false (위 조건 충족 못한 곳)
```
→ 두 번째 else if 이후로 도달 불가. **로직 의심 — v2 재작성 시 명확하게**

### B3. `Note.ReadJudge` `checkType == 0` 분기에서 lane 범위 체크
```csharp
if( lane >= line && lane <= line + length)  // 폐구간 [line, line+length]
```
→ length=7이면 8 lane 차지. v2 spec은 `length` = 가로 폭이라 `[line, line+length-1]` 가 맞을 듯. **인덱싱 일관성 검증**

### B4. `HoldNoteBody.ReadJudge` curJudge==count-1 일 때
- count=2인 홀드의 경우 curJudge가 0에서 바로 1(==count-1)로 가는데 첫 노트 판정이 0에서 처리됨
- count=1 (단일 노트?)는 분기 어디로 가는지 모호

### B5. `GamePlayer` 노트 판정 후 `RemoveAt(j)` + 루프 계속
```csharp
for (int j = 0; j < inGameNote.Count; j++) {
    ...
    inGameNote.RemoveAt(j);  // j 이후 인덱스 shift
    Destroy(temp);
    break;
}
```
→ 다행히 `break`가 있어서 같은 lane에선 1프레임당 1개 처리. 하지만 다음 lane 루프에서 같은 노트 다시 봄. **의도가 맞는지 확인**

### B6. `SetUp()` 의 빈 try-catch
```csharp
catch (Exception e) { 
    // empty
}
```
→ 모든 예외 무시. 디버깅 악몽. **로깅 추가 또는 catch 제거**

---

## 🎯 매직 넘버 / 네이밍

### M1. `Note.cs` 매직 넘버 폭격
| 값 | 위치 | 의미 추정 |
|---|---|---|
| `10` | height 초기값 | 출발선 y 좌표 (world) |
| `-2.25` | posVector.y | 판정선 y 좌표 |
| `-0.08` | cal 계산 | 원근 효과 계수 |
| `0.8` | scaleVector.x | 노트 가로 스케일 |
| `3.2` | scaleVector.y | 노트 세로 스케일 (Drop 중) |
| `0.64` | Set scale.y | 노트 세로 스케일 (정적) |
| `0.16` | Set scale.x | 노트 가로 스케일 (정적) |
| `-10.5` | lanePos | 화면 좌측 가장자리? |
| `7.75` | posVector.y | 출발선 (height>10) |
| `480, 5` | speed 분모 | Drop 속도 계수 |
| `6.4` | SetJudge factor | position step → 게임 시간 변환 |

→ `static class NoteConstants` 또는 ScriptableObject 로 추출. 각 상수 의미 주석

### M2. `Note.height` 이름 — 사실은 "현재 y 위치"
- `height = 10`에서 출발 → `0`이 판정선 → 음수 = 지나침
- **세로 길이가 아니라 위치**. v2에서는 `currentY` 또는 `posY` 권장

### M3. `Note.ReadJudge` checkType — int 매직 넘버
```csharp
// checkType 0: MissCheck, 1: press, 2: slide, 3: intouch, 4: endtouch, 5: flickUp, 6: flickDown, 100: Miss
```
→ **반드시 enum으로**. 주석으로 의미 전달 = 코드 냄새
- 100=Miss는 외부에서 명시적 Miss 강제? 아무도 안 부르는 것 같음

### M4. `JudgementType` enum에 결과 + 상태 혼재
```csharp
Perfect, Great, Good, Miss, SpMiss, Checked, NotChecked
```
- `Checked` / `NotChecked` 는 판정 **결과**가 아니라 **처리 상태** — 분리 권장
- `SpMiss` (Special Miss, HoldNote endtouch 실패) — v2엔 통합 권장

### M5. `Define.JudgementType.SpMiss` — 별도 등급 의문
- HoldNote 마지막 endtouch에서만 발생
- 그 후 별도 `secondJudgeChecker` 로 Miss로 또 처리 (코드 복잡)
- v2: 단순 Miss로 통합 → secondJudgeChecker 제거 가능

### M6. `GamePlayer.bpm = 120 / 5 * 8 = 192`
- 이름이 `bpm`인데 실제 곡 BPM이 아니라 시간 증가 계수
- `120` (BPM) × `8` (?) / `5` (?) — 의미 불명
- v2: 이름 변경 (`timeUnitsPerSec` 등) + 곡 BPM과 분리

### M7. `chartToolOffset = 6.4f`
- 채보툴 position step → 게임 시간 변환 상수
- 120 BPM 가정 (60sec / 192step × 16 = ...)
- v2: BPM 동적이면 공식으로 유도, 이름도 명확하게

### M8. `playerSpeed = 4`
- 배속 같은데 r 변수도 아니고 단위 모호
- v2: r ∈ [0.1, 3.0] spec 따라 명확화

### M9. `cardEffect[7]`, lane 배열 `[21]` 등
- 슬롯 수가 코드 곳곳에 박혀있음
- v2: `const int SLOT_COUNT = 8`, `const int LANE_COUNT = 24` 한 곳에서 관리

### M10. `flickMinDis = 0.2f` 선언만 — 사용 안 됨
- NoteScreen.cs dead variable

### M11. `AudioTest.ct > 2` — 2초 매직
- 곡 시작 전 대기 시간. spec 없음
- v2: 곡 메타데이터에서 오프셋으로 통합

---

## 🏗 구조 개선

### S1. `BasicNote / SlideNote / FlickNote / HoldNote` — 단순 래퍼
- 모두 `base` 호출만 하는 메서드들
- 다형성이 의미 있는 곳은 ReadJudge의 checkType 분기 뿐
- v2: 상속 깊이 줄이거나 `INoteJudge` 인터페이스 + 전략 패턴

### S2. `Note.ReadJudge` if-else 폭포 8단계
- 8가지 등급 분기 + lane 범위 + checkType 분기
- 함수 길이 60줄
- v2: 분리:
  - 거리 계산 함수
  - 등급 분류 함수 (lookup 또는 switch)
  - lane/checkType 검증 함수

### S3. `GamePlayer` — God Class
- 책임 7가지: 채보 로드, 노트 스폰, Drop, 판정, 점수, UI 텍스트, 카드 매니저 호출
- 파일 570줄, 단일 클래스
- v2 분리 권장:
  - `ChartLoader` (Json → 4종 노트 정보)
  - `NoteSpawner` (timing 따른 인스턴스화)
  - `NoteDropper` (모든 노트 Drop 호출)
  - `JudgeProcessor` (touch 입력 + ReadJudge + HitEvent 생성)
  - `ScoreApplier` (HitEvent → Managers.Score)
  - `GameLoop` (orchestrator)

### S4. `HoldNoteBody.ReadJudge` 상태머신 1메서드
- `if (curJudge == 0) … else if (curJudge == count-1) … else …` 의 80줄
- 각 상태별 메서드 분리: `JudgeFirst()`, `JudgeMid()`, `JudgeLast()`

### S5. `NoteScreen.Update` 모놀리식
- 단일 Update에 터치 등록 + Raycast + 각도 + GamePlayer 직접 쓰기 다 들어있음
- v2 분리:
  - `TouchTracker` (touch ID 매핑)
  - `LaneRaycaster` (touch → laneNum)
  - `FlickDetector` (각도 → 방향)
  - `InputForwarder` (이벤트로 GamePlayer 알림)

### S6. `Managers` Singleton 강결합
- 매니저 13개 동시 생성 (Sprint 1에 일부만 필요)
- 테스트 어려움
- v2: 지연 초기화 + interface 분리 (또는 단순 ServiceLocator)

### S7. `JsonManager` — Load + Return 2단계
```csharp
Managers.Json.LoadJson();
noteJson = Managers.Json.ReturnJson();
```
→ `var noteJson = Managers.Json.Load(path);` 단일 호출이 깔끔

### S8. `ScoreContext` 만들고 안 씀
- `ApplyNoteScore`에서 `ScoreContext ctx = new ...` 만들고 hook 호출
- 그런데 **최종 점수 계산은 ctx 무시하고 직접 식**으로 함
- ctx의 의미 = 카드 hook 들이 ctx 수정하지만 결과 반영 안 됨? **버그 또는 의도 모호**

### S9. `GameScoreInfo` 책임 혼재
- 점수 + UI 텍스트 갱신 + JudgementText 호출 + 카드 매니저 접근
- v2: 점수 적용은 ScoreManager에 위임, UI는 별도 컴포넌트

### S10. 카드 효과 적용 패턴
- `HitEvent.cardEffect[0..6]` boolean → `cardSets[i].OnNoteTrigger(hitEvents[i])` 7번 if
- 주석 처리된 코드 (현재 안 씀)
- v2 카드 도입 시 — 단순 for loop 또는 Observer 패턴

### S11. UniTask 사용 (GamePlayer)
```csharp
public async UniTask GameSystem() {
    while (true) {
        ...
        await UniTask.WaitUntil(() => play);
    }
}
```
- Update 대안. 일시정지 자연스럽게 처리하는 장점
- 단점: Cysharp.Threading.Tasks 의존성 추가
- v2: 유지 가능하지만 Update + bool play 로 단순화 가능 (트레이드오프)

---

## ⚡ 성능 / GC

### P1. `HoldNoteBody.DrawLine` 매 프레임 `new AnimationCurve()`
```csharp
AnimationCurve widthCurve = new AnimationCurve();
for (...) widthCurve.AddKey(...);
lineRenderer.widthCurve = widthCurve;
```
→ 매 프레임 객체 할당 + LineRenderer 재할당. **GC 압박**
- v2: 한 번 만들고 키만 갱신 또는 CalculateWidth 캐싱

### P2. `Note.Drop` 매 프레임 `Vector3 scaleVector/posVector` 사용
- 클래스 필드라 재사용은 OK
- 다만 가독성 ↓ (왜 필드인지 불명) — 로컬 변수 + struct 채우기가 명료

### P3. `NoteScreen` `Camera.main` 호출
```csharp
Ray ray = Camera.main.ScreenPointToRay(touchPos);
```
- Camera.main = `FindWithTag("MainCamera")` 비용
- 5개 터치 × 매 프레임
- v2: `cachedCamera = Camera.main` Start에서 캐싱

### P4. NoteCreator 매번 `Instantiate` + GamePlayer 매번 `Destroy`
- 풀링 미구현 ("Need Pooling" 주석)
- 빠른 곡일수록 GC 압박
- v2: ObjectPool 도입

### P5. `GamePlayer` 판정 루프 24 lane × inGameNote
- 모든 lane × 모든 노트 = O(N²) per frame
- 노트가 100개씩 동시 화면에 있으면 24×100=2400 회/frame
- v2: lane별 노트 그룹핑 또는 spatial partitioning. 또는 빠른 lane 매칭 (line 기준 정렬)

### P6. `Managers.Score.ApplyNoteScore` 매번 `new ScoreContext`
- 매 노트마다 객체 할당
- v2: pool 또는 struct로 변경

### P7. `HitEvent` 매번 `new HitEvent(judge, line, line+length)`
- 매 판정마다 생성 + cardEffect[7] 배열도 매번 생성
- 빠른 채보에서 GC 빈도 ↑
- v2: pool 또는 struct (현재 class)

---

## 🧹 Dead Code / 미사용

### D1. 미사용 import
- `using NUnit.Framework;` — NoteJson.cs / GamePlayer.cs / GameScoreInfo.cs (테스트 프레임워크, production에 필요 없음)
- `using static HitEvent;` — GamePlayer.cs (HitEvent static 멤버 없음)

### D2. 주석 처리된 코드 블록
- `GameScoreInfo.CardScoring`에 카드 인덱스 0~6 if-else 7회 (전체 주석)
- `GamePlayer` reset region에 switch (judgeChecker[i]) Plain Mode 점수 적용 (전체 주석)
- `HoldNoteBody.DrawLine` 위에 이전 버전 (주석)
→ 정리

### D3. `flickMinDis = 0.2f` (NoteScreen) — 선언만, 사용 X

### D4. `Note.SpMiss` 단일 사용처
- HoldNote endtouch에서만. 분기도 별도 처리.
- v2: 통합 Miss로

### D5. `Managers.MainGameManager` — 인스턴스만 있고 public 프로퍼티 없음
- 외부에서 접근 불가 → dead

### D6. `AudioTest.timer` public field 미사용
- 인스펙터 노출만 의도? 코드에서 안 씀

### D7. `Debug.Log("Hold height is " + height)` (NoteCreator)
- 프로덕션 출력. 정리 또는 conditional compile

### D8. `Note.endFingerID = -1` initial — 사용처 빈약
- pressFingerID/fingerID도 비슷. v1에서 multi-touch 처리하려고 도입했으나 활용도 낮음

---

## v2 리팩터링 적용 순서 (권장)

salvage 작업 중 함께:

1. **상수 추출** — 매직 넘버들을 `GameConstants` (또는 ScriptableObject)로 모음
2. **enum 도입** — `CheckType`, `JudgementResult`, `NoteDirection` 등 int → enum
3. **JudgementType 분리** — 결과(Perfect/Great/Good/Miss) vs 상태(Checked/NotChecked/Skipped)
4. **lane/slot 수 상수화** — `const int SLOT_COUNT = 8; const int LANE_COUNT = 24;`
5. **풀링 도입** — Note, HitEvent (선택, Sprint 1엔 후순위)
6. **God Class 분리** — GamePlayer를 ChartLoader/Spawner/Dropper/Judger/Scorer/Loop 로 split
7. **빈 catch 제거 / 로깅** — SetUp 등
8. **Dead code 정리** — 미사용 import, 주석 블록, 미사용 필드
9. **버그 검증** — B1~B6 항목 의도 확인 후 수정

---

## 변경 이력

- 2026-05-16: v1 8개 파일 (Note, HoldNoteBody, NoteCreator, NoteScreen, Bar, JsonManager, ScoreManager, GameScoreInfo, HitEvent, AudioTest, Managers, GamePlayer) 점검 후 신설. 잠재 버그 6건 + 매직 넘버 11건 + 구조 11건 + 성능 7건 + dead code 8건 정리.
