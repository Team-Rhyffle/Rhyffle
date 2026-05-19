# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Sprint 1.5 완료** — 10/10 task DONE. Phase push 완료.

- Sprint 1 (2026-05-18 완료): 14/14 task + Steelman 게이트 3개 PASS.
- Sprint 1.5 (2026-05-19 완료): 10/10 task DONE. EventBus + 이벤트 struct 4종 + CardData DTO + ScoreModifier + ICardEffect 5-method + CardSystem + CardEffectRegistry + 더미 카드 3종 + bootstrap + PokerHandEvaluator + tests + CardBoardUI + 씬 배치 + GameLoop 통합 + 통합 검증 + Steelman 게이트 2개 PASS. **36/36 EditMode tests PASS**.

## 다음 액션
**Sprint 2 진입 준비** — 진입 조건 확인:
1. `김한울 카드 데이터셋 도착 확인` (마감 2026-05-17, 지연 중) — 데이터셋 없으면 일반능력 system spec 미확정
2. 일반능력 system spec 확정 후 `ICardEffect` 실구현 + `CardSystem` 확장
3. 묘지/덱 spec 확인 (`SPEC.md` 인덱스 경유)

## 블로커
**Sprint 2 진입 블로커**: 김한울 카드 데이터셋 미도착 (2026-05-17 마감 → 초과). 진행 상황 확인 필요.

## 펜딩 (사용자 / 외부)
- 카드 데이터셋 (≈400장) — 한울 2026-05-17 마감 예정 (Sprint 2 입력 자산). **진행 상황 확인 필요**
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**: JSON 채보 드롭 검증 끝난 뒤
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 김민주 (채보) 더미 채보 데이터 — 본인이 직접 작성하거나 김민주 요청. Sprint 1 Week 1 step 9 시점

## 최근 변경
- 2026-05-19: **Sprint 1.5 Task 10 완료 + Phase push** — 통합 검증 PASS (score 1,320,000 / 1,584,000), 36/36 EditMode tests PASS, Steelman Gate A+B PASS, CardBoard UI 확인. CURRENT_STATUS + DECISIONS 갱신. SteelmanGatesTest.cs 신설.
- 2026-05-19: **Sprint 1.5 Task 9 완료** (40b04ca) — GameLoop EventBus 통합 (modifier 합산 + 콤보 보호 + 키노트 stub).
- 2026-05-19: **Sprint 1.5 Task 8 완료** (e0d3091) — CardBoardUI (7 슬롯 procedural, FieldChangedEvent 구독) + SceneBootstrap EnsureCardSystemAndBoard helper + 메뉴 아이템.
- 2026-05-19: **Sprint 1.5 Task 7 완료** (39e811f) — PokerHandEvaluator stub + PokerHand enum 13값 + 4 EditMode tests.
- 2026-05-19: **Sprint 1.5 Task 6 완료** (48721b4) — 더미 카드 3종 + DummyEffectsBootstrap + 8 EditMode tests.

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
