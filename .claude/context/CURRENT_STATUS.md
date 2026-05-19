# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Sprint 1.5.1 진행 중** — T1 완료 (8944a12). T2~T5 대기.

- Sprint 1 (2026-05-18 완료): 14/14 task + Steelman 게이트 3개 PASS.
- Sprint 1.5 (2026-05-19 완료): 10/10 task DONE. **36/36 EditMode tests PASS**.
- Sprint 1.5.1 (2026-05-19 진행): Lane Visualization Refinement + Card-Lane Mapping. T1 완료.

## 다음 액션
**Sprint 1.5.1 T2**: `GameConfig.BarX` 공식 업데이트 (n-12.5 → n-13), `GameConfig.LaneX(i)` 신규 헬퍼 추가, SceneBootstrap 하드코드 `n-13` → `GameConfig.BarX(n)` 교체.

## 블로커
**Sprint 2 진입 블로커**: 김한울 카드 데이터셋 미도착 (2026-05-17 마감 → 초과). 진행 상황 확인 필요.

## 펜딩 (사용자 / 외부)
- 카드 데이터셋 (≈400장) — 한울 2026-05-17 마감 예정 (Sprint 2 입력 자산). **진행 상황 확인 필요**
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**: JSON 채보 드롭 검증 끝난 뒤
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 김민주 (채보) 더미 채보 데이터 — 본인이 직접 작성하거나 김민주 요청. Sprint 1 Week 1 step 9 시점

## 최근 변경
- 2026-05-19: **Sprint 1.5.1 T1 완료** (8944a12) — LaneAnchor.cs 신규, Bar.cs 시각 전용화, NoteScreen.cs lanes 추가, SceneBootstrap 25 Bar + 24 LaneAnchor, Game.unity 재생성. 36/36 tests PASS.
- 2026-05-19: **Sprint 1.5 Task 10 완료 + Phase push** — 통합 검증 PASS (score 1,320,000 / 1,584,000), 36/36 EditMode tests PASS, Steelman Gate A+B PASS.
- 2026-05-19: **Sprint 1.5 Task 9 완료** (40b04ca) — GameLoop EventBus 통합 (modifier 합산 + 콤보 보호 + 키노트 stub).
- 2026-05-19: **Sprint 1.5 Task 8 완료** (e0d3091) — CardBoardUI (7 슬롯 procedural, FieldChangedEvent 구독) + SceneBootstrap EnsureCardSystemAndBoard helper.
- 2026-05-19: **Sprint 1.5 Task 7 완료** (39e811f) — PokerHandEvaluator stub + PokerHand enum 13값 + 4 EditMode tests.

## 작업 재개 시 첫 행동
1. **이 파일 (CURRENT_STATUS.md) 먼저 읽기**
2. RHYFFLE_CONTEXT.md auto-inject 확인 (SessionStart hook)
3. 필요 시 SPEC.md 인덱스 확인
4. 위 "다음 액션" 으로 즉시 진행
5. 작업 진행 / 결정 / 블로커 발생 시 이 파일 갱신

## 갱신 가이드라인
- **한 두 줄 수준**으로 압축. 긴 설명은 DECISIONS.md
- "다음 액션" 은 항상 *구체적*으로: 파일명 / 함수명 / step 번호
- "최근 변경" 은 최신 5개만 유지. 오래된 건 DECISIONS.md 로 흘려보냄
- v2 코드 진행도: 파일 단위 (예: "GameConfig.cs 완료, ChartLoader 진행 중")
