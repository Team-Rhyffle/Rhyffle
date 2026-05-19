# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Sprint 1.5 진행 중** — Task 1 완료 (66c8c4b), Task 2 완료 (91eb7f0), Task 3 완료 (1a88476), Task 4 완료 (5504e08), Task 5 완료 (b5ef125), Task 6 완료 (48721b4).

- Sprint 1 (2026-05-18 완료): 14/14 task + Steelman 게이트 3개 PASS.
- Sprint 1.5 plan (30d0804): `docs/superpowers/plans/2026-05-18-sprint1.5-card-framework-lofi-ui.md` — 10 task. **Task 1–6 완료**: EventBus + 이벤트 struct 4종 + CardData DTO + ScoreModifier + ICardEffect + CardSystem + CardEffectRegistry + 더미 카드 3종 + bootstrap + 8 tests. Task 7+ 남음.

## 다음 액션
**Sprint 1.5 Task 7+** — plan 파일 확인: `docs/superpowers/plans/2026-05-18-sprint1.5-card-framework-lofi-ui.md`.

## 블로커
없음. 다만 Sprint 2 진입 (실제 카드 시스템) 위해선 **김한울 카드 데이터셋 도착 필요** (마감 2026-05-17 = 어제) — 진행 상황 확인 권장. Sprint 1.5는 데이터셋 무관, 즉시 진행 가능

## 펜딩 (사용자 / 외부)
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**: JSON 채보 드롭 검증 끝난 뒤
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 김민주 (채보) 더미 채보 데이터 — 본인이 직접 작성하거나 김민주 요청. Sprint 1 Week 1 step 9 시점
- 카드 데이터셋 (≈400장) — 한울 2026-05-17 마감 예정 (Sprint 2 입력 자산). **진행 상황 확인 필요**

## 최근 변경
- 2026-05-19: **Sprint 1.5 Task 6 완료** (48721b4) — 더미 카드 3종 (DummyAtk_SpadeK/DummyDef_HeartQ/DummySup_DiamondJ) + DummyEffectsBootstrap + 8 EditMode tests. 8/8 PASS.
- 2026-05-19: **Sprint 1.5 Task 5 완료** (b5ef125) — CardSystem 본체 + CardEffectRegistry + ICardEffect 5-member 확장. 0 compile errors.
- 2026-05-19: **Sprint 1.5 Task 4 완료** (5504e08) — ICardEffect 3-member stub + CardSystem MonoBehaviour stub. 0 compile errors.
- 2026-05-19: **Sprint 1.5 Task 2 완료** (91eb7f0) — 이벤트 struct 4종 (NoteJudgedEvent/ComboChangedEvent/FieldChangedEvent/KeyNoteProcessedEvent). `Assets/Scripts/Events/` 추가. 0 compile errors.
- 2026-05-19: **Sprint 1.5 Task 3 완료** (1a88476) — CardData DTO + CardGroup/Suit/Rank/Rarity 4 enums. `Assets/Scripts/Cards/` 신설. 0 compile errors.
- 2026-05-18: **Sprint 1.5 plan 작성** (30d0804) — Card Effect Framework + Lo-Fi UI Placement. 10 task, EventBus pub/sub + 더미 카드 3종 발동 검증 + CardBoard 7 슬롯 placeholder.
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
