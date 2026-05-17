# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Sprint 1 구현 진행 중 — 12 / 14 task 완료 (.cs + asset). 나머지 2 task (씬 셋업 / 통합 검증) Unity Editor 응답 대기 중.**

완료 (12개):
- Task 0: URP→Built-in 다운그레이드
- Task 1: GameConfig + 5 tests
- Task 2: ChartData DTO
- Task 3: ChartLoader + 3 tests
- Task 4: Conductor (dspTime + PlayScheduled) + 3 tests
- Task 5: Note 통합 클래스
- Task 6: HoldNoteBody
- Task 7: JudgeProcessor + 6 tests
- Task 8: Bar + NoteScreen
- Task 9: GameLoop 오케스트레이터 (200줄, Spawn/Drop/Input/Score/Combo 내부 메서드)
- Task 10: v1 prefab/sprite salvage (5 prefab + .mat + Pause sprite 3장 + FlickNote 4 variant)
- Task 12: dummy_test.json 더미 채보 (4종 노트 + 4방향 flick, 17초)
- Task 11 부분: SceneSetup Editor 메뉴 스크립트 (24 Bar 자동 생성)

블록 (Unity 응답 대기):
- Task 11 나머지: Game.unity 씬 파일 생성, Camera/Canvas/AudioSource 셋업, GameLoop 인스펙터 와이어링, v1 prefab missing script 교체
- Task 13: 통합 검증 + Steelman 게이트 3개

## 다음 액션
**Unity Editor 응답 복구 후**:
1. `Rhyffle/Add Bar Tag` 메뉴 실행 → Tag 'Bar' 추가
2. Game.unity 씬 생성 (Camera/Canvas/EventSystem/GameSystem/NoteScreen/NoteBoard/GameLoop/AudioSource 6 루트)
3. `Rhyffle/Setup 24 Bars` 메뉴 실행 → 24 Bar 자동 생성
4. Canvas 자식 UI (Score/JudgementText/Combo/PauseCountdown) 수동 셋업
5. GameLoop 인스펙터에 8 prefab ref + 컴포넌트 ref 드래그
6. v1 prefab (BasicNote/SlideNote/FlickNote/HoldNote/HoldNoteBody) missing script 제거 + Note 컴포넌트 추가
7. FlickNote 4 variant 회전 설정 (Up=0/Right=-90/Down=180/Left=90)
8. Play Mode 진입 → JSON 채보 17초 드롭 시각 검증
9. Steelman 게이트 3개 (dspTime drift 60초 / 24 Bar Raycast 인접 hit / Canvas z-fight)

## 블로커
**Unity Editor 응답 안 함** — URP 17.2.0 제거 + 9 .cs 추가 + 16 asset 추가 후 import에서 hang 의심. instance MCP 연결은 유지되나 editor_state 응답 timeout (60s × 다회). 사용자가 Unity 창 focus 또는 재시작 필요할 수 있음.

## 펜딩 (사용자 / 외부)
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**: JSON 채보 드롭 검증 끝난 뒤
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 김민주 (채보) 더미 채보 데이터 — 본인이 직접 작성하거나 김민주 요청. Sprint 1 Week 1 step 9 시점
- 카드 데이터셋 (≈400장) — 한울 2026-05-17 마감 예정 (Sprint 2 입력 자산). **진행 상황 확인 필요**

## 최근 변경
- 2026-05-17: **Sprint 1 구현 시작 — 12 / 14 task 완료** (.cs 9개 + tests 17개 + asset 22개 + 더미 채보). Unity Editor 응답 대기로 씬 셋업/통합 검증 보류. SceneSetup Editor 메뉴 스크립트 준비 (Unity 복구 시 24 Bar 자동 생성).
- 2026-05-17: **v1 Game.unity 씬 인스펙션 (Unity MCP)** — V1_SCENE_INSPECTION.md 신설. Camera ortho size 10 / Bar 21개 간격 1.0unit / 판정선 y=-2.5 실측 (placeholder -2.25 정정) / Canvas ScreenSpaceCamera 956×440 / Pause 카운트다운 3-2-1 sprite / 노트 prefab 5종 경로 확정. V2_ARCHITECTURE + V2_ARCHITECTURE_SPRINT1 에 인스펙션 결과 통합 (씬 셋업 step 구체화 + 매핑표 보강)
- 2026-05-17: Sprint 1 spec 잔여 빈칸 전원 확정 — HoldNote SpMiss v1 로직 그대로 / 라인 가로폭 화면 fit / 플릭 시간창 없음 / r 옵션 X / 점수·Pause UI v1 참고 / 콤보 UI 중앙상단. JUDGMENT_SPEC + V2_ARCHITECTURE_SPRINT1 갱신
- 2026-05-17: Week 1 우선순위 명시 — 음원/곡 메타 후순위, JSON 채보 드롭 시각 검증 우선
- 2026-05-16: Council (5 agents, 2 rounds) — V2_ARCHITECTURE_SPRINT1.md 신설 (14→6 컴포넌트 점진 분해)
- 2026-05-16: placeholder 값 임의 결정 (h=1.0, 거리=노트중심, 판정선 y=-2.25 등)
- 2026-05-16: spec 문서 풀세트 작성 (CHART_SPEC, JUDGMENT_SPEC, V1_SALVAGE_MAP, REFACTOR_NOTES, V2_ARCHITECTURE)

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
