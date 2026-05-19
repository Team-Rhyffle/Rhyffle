# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Sprint 1.5.6 T2 완료** — SceneBootstrap DeckSystem wiring. **62/62 PASS**.

- Sprint 1 (2026-05-18 완료): 14/14 task + Steelman 게이트 3개 PASS.
- Sprint 1.5 (2026-05-19 완료): 10/10 task DONE. 36/36 EditMode tests PASS.
- Sprint 1.5.1 (2026-05-19 완료): 5/5 task DONE (Lane Visualization + Card-Lane Mapping). 40/40 tests PASS.
- Sprint 1.5.2 (2026-05-19 완료): 5/5 task DONE. 58/58 tests PASS.
- Sprint 1.5.3 (2026-05-19 완료): 5/5 task DONE. HUD 재배치 + 5 placeholder + 3 영역 visual band. 58/58 tests PASS.
- Sprint 1.5.4 (2026-05-19 완료): NotoSansKR SDF (Apache 2.0) + atlas sub-asset 저장. 58/58 tests PASS.
- Sprint 1.5.5 (2026-05-19 완료): PauseButton wire + code polish + DECISIONS.md 정리.
- Sprint 1.5.6 T1 (2026-05-19): DeckSystem.cs 신규 (단일 묘지, Fisher-Yates, KeyNote trigger). 62/62 PASS.
- Sprint 1.5.6 T2 (2026-05-19): SceneBootstrap EnsureCardSystemAndBoard에 DeckSystem 생성 + cardSystem wire. 62/62 PASS.

## 다음 액션
**Sprint 1.5.6 T3** — 다음 task 확인 필요.

## 블로커
**Sprint 2 진입 블로커**: 김한울 카드 데이터셋 미도착 (2026-05-17 마감 → 초과). 진행 상황 확인 필요.

## 펜딩 (사용자 / 외부)
- 카드 데이터셋 (≈400장) — 한울 2026-05-17 마감 예정 (Sprint 2 입력 자산). **진행 상황 확인 필요**
- A-high straight (10-J-Q-K-A) spec — Sprint 2 시 확정
- 묘지/덱/키노트/일반능력 spec — Sprint 2 입력 대기
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**: JSON 채보 드롭 검증 끝난 뒤
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 곡 정보 데이터 wire (chartName + audio 메타 spec — 펜딩)
- 셔플 기능 (Sprint 2 카드 spec 도착 시)
- 덱묘지 데이터 표시 (Sprint 2)
- Figma Hi-Fi 외관 교체 (placeholder → 실제 art)

## 최근 변경
- 2026-05-19: **Sprint 1.5.6 T2 완료** (c442ce9) — SceneBootstrap EnsureCardSystemAndBoard에 DeckSystem 생성 + cardSystem wire. 62/62 PASS.
- 2026-05-19: **Sprint 1.5.6 T1 완료** (c1b0a5c) — DeckSystem.cs 신규 (단일 묘지, Fisher-Yates, KeyNote trigger) + 4 tests. 62/62 PASS.
- 2026-05-19: **Sprint 1.5.5 T3 완료** + Phase push — code polish (GameConfig 상수 2개 보존 + 주석 명확화) + DECISIONS.md Sprint 1.5.x 정리.
- 2026-05-20: **Sprint 1.5.5 T2 완료** (d130af1) — NotoSansKR SDF (Apache 2.0) 교체, MalgunGothic 삭제 (license-clean). SceneBootstrap 2곳 경로 교체. 58/58 PASS.
- 2026-05-20: **Sprint 1.5.5 T1 완료** — PauseButton.cs 신규 + GameLoop.IsPaused getter + SceneBootstrap PausePlaceholder wire. 58/58 PASS.

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
