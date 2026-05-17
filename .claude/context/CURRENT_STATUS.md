# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Sprint 1 Plain Mode (PlayScreen) 완료. 14 / 14 task PASS + Steelman 게이트 3개 PASS.** Phase push 대기.

검증 결과 (manual frame ticking via Unity MCP execute_code, runInBackground 제약 우회):
- 16 / 16 노트 Perfect, sprite.y = -2.500 (== JUDGE_LINE_Y) 정확 매치
- score 1,200,000, combo 1→16 정상
- Gate 1 (dspTime drift 60s): drift ~14ms/52s — Conductor SoT가 dspTime이라 wall clock drift 판정 무관 PASS
- Gate 2 (Bar adjacent raycast): 24/24 lane 정확 hit, mismatch 0 PASS
- Gate 3 (Canvas z-fight): 4 UI 모두 localZ=0 동일 plane, sibling idx로만 차등, ScreenSpaceCamera라 z-fight 발생 안 함 PASS

세션 중 발견·수정:
- 첫 4 노트 Miss → startDelay=2s + Conductor lead 0.3s (3ca1c16)
- AutoJudge 19ms 조기 발화 (sprite -2.31 ~ -2.40) → `songTime >= noteTime`으로 변경 (6db79e1)
- Flick 4종 + Slide root sprite NULL (전부 invisible) → Flick(U/D)_0 / Basic_0 할당 (af5daa6)
- Flick(D) PPU 100 → 888 (단독 9× 거대) (0a8b3f8)

## 다음 액션
1. Phase push: `git push origin main` — Sprint 1 plain mode 전체 commit (28개) 외부 공유
2. 다음 spec 페이즈 진입 — 김한울 카드 데이터셋 마감 (2026-05-17) 확인 → Sprint 2 입력 자산 준비

## 블로커
없음.

## 펜딩 (사용자 / 외부)
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**: JSON 채보 드롭 검증 끝난 뒤
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 김민주 (채보) 더미 채보 데이터 — 본인이 직접 작성하거나 김민주 요청. Sprint 1 Week 1 step 9 시점
- 카드 데이터셋 (≈400장) — 한울 2026-05-17 마감 예정 (Sprint 2 입력 자산). **진행 상황 확인 필요**

## 최근 변경
- 2026-05-18: **Sprint 1 Plain Mode 14/14 task + 3 Steelman 게이트 PASS** — Unity MCP execute_code의 reflection-based manual ticking으로 headless 검증 (Editor의 OS focus 의존 제약 우회). 16 노트 Perfect, sprite/judgment 정렬 검증. Flick 4종 + Slide invisible 버그 + Flick(D) 9× scale 버그 발견·수정.
- 2026-05-17: Sprint 1 구현 — .cs 9개 + tests 17개 + asset 22개 + 더미 채보 + Game.unity 6 루트 셋업.
- 2026-05-17: v1 Game.unity 씬 인스펙션 (Unity MCP) — V1_SCENE_INSPECTION.md 신설. Camera ortho size 10 / Bar 21개 간격 1.0unit / 판정선 y=-2.5 실측.
- 2026-05-17: Sprint 1 spec 잔여 빈칸 전원 확정 — HoldNote SpMiss v1 로직 / 화면 fit / 플릭 시간창 없음 / r 옵션 X / UI 위치.
- 2026-05-16: Council (5 agents, 2 rounds) — V2_ARCHITECTURE_SPRINT1.md 신설 (14→6 컴포넌트 점진 분해)

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
