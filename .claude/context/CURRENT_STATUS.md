# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Sprint 1.5 진행 중** — Task 1 완료 (66c8c4b). Task 2 대기.

- Sprint 1 (2026-05-18 완료): 14/14 task + Steelman 게이트 3개 PASS.
- Sprint 1.5 plan (30d0804): `docs/superpowers/plans/2026-05-18-sprint1.5-card-framework-lofi-ui.md` — 10 task. **Task 1/10 완료**: EventBus generic pub/sub (`Assets/Scripts/Events/EventBus.cs`, 4 EditMode tests). Unity MCP 세션 unavailable로 test runner 미확인 (파일·.meta 생성 확인, compile 오류 미확인).

## 다음 액션
**Sprint 1.5 Task 2** — `JudgmentResult` struct 구현 (`Assets/Scripts/Events/JudgmentResult.cs`). EventBus 연동 준비.

## 블로커
없음. 다만 Sprint 2 진입 (실제 카드 시스템) 위해선 **김한울 카드 데이터셋 도착 필요** (마감 2026-05-17 = 어제) — 진행 상황 확인 권장. Sprint 1.5는 데이터셋 무관, 즉시 진행 가능

## 펜딩 (사용자 / 외부)
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**: JSON 채보 드롭 검증 끝난 뒤
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 김민주 (채보) 더미 채보 데이터 — 본인이 직접 작성하거나 김민주 요청. Sprint 1 Week 1 step 9 시점
- 카드 데이터셋 (≈400장) — 한울 2026-05-17 마감 예정 (Sprint 2 입력 자산). **진행 상황 확인 필요**

## 최근 변경
- 2026-05-18: **Sprint 1.5 plan 작성** (30d0804) — Card Effect Framework + Lo-Fi UI Placement. 10 task, EventBus pub/sub + 더미 카드 3종 발동 검증 + CardBoard 7 슬롯 placeholder. 카드 spec은 Notion 카드시스템 페이지에서 추출 (필드 7장, 좌→우 발동, 카드군 5종, 족보 12종). 5 stub 명시 (묘지/키노트/일반능력/카드UI/카드source).
- 2026-05-18: **Sprint 1 phase push 완료** — origin/main b0d7e5c (31 commits 외부 공유). Sprint 1 plain mode 종결.
- 2026-05-18: **Sprint 1 Plain Mode 14/14 task + 3 Steelman 게이트 PASS** — Unity MCP execute_code reflection-based manual ticking으로 headless 검증. 16노트 Perfect. Flick 4종 + Slide invisible / Flick(D) 9× scale 버그 발견·수정.
- 2026-05-17: Sprint 1 구현 — .cs 9개 + tests 17개 + asset 22개 + 더미 채보 + Game.unity 6 루트 셋업.
- 2026-05-17: v1 Game.unity 씬 인스펙션 (Unity MCP) — V1_SCENE_INSPECTION.md 신설. Camera ortho size 10 / Bar 21개 간격 1.0unit / 판정선 y=-2.5 실측.

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
