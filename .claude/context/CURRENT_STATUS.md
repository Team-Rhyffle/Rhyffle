# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Spec / Architecture 완료, Sprint 1 구현 진입 직전.**

v2 코드 = 0줄. `RhyffleV2/Assets/Scripts/` 비어있음.
모든 spec / 매핑 / 청사진 / placeholder 결정 완료. 즉시 구현 시작 가능.

## 다음 액션
**V2_ARCHITECTURE_SPRINT1.md §3 Week 1 step 1 → GameConfig 신규 작성**

상세 순서 (Week 1):
1. `GameConfig` (상수 + enum 분리) — 2h
2. `ChartData` (v1 NoteType.cs salvage + noteType 제거) — 1h
3. `ChartLoader` (v1 JsonManager salvage + 단일 호출) — 1h
4. `Conductor` (신규, dspTime, BPM 하드코드) — 3h
5. `Note` (4종 통합) + `HoldNoteBody` — 5h
6. `JudgeProcessor` (거리 기반 신규) — 3h
7. `GameLoop` (v1 GamePlayer slim + 카드 호출 제거) — 4h
8. Unity 씬 셋업 (24-lane Bar 배치) — 4h
9. 더미 채보 데이터 — 2h
10. 통합 테스트 — 4~8h

## 블로커
**없음.** 미확정 spec 3건 (h / 거리 기준점 / 곡 메타) 은 placeholder 값으로 우회 확정 (2026-05-16 결정).

## 펜딩 (사용자 / 외부)
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 수정 예정. 도착 시 `Conductor` 동적화 + `JUDGMENT_SPEC.md` 갱신
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 김민주 (채보) 더미 채보 데이터 — 본인이 직접 작성하거나 김민주 요청. Sprint 1 Week 1 step 9 시점
- 카드 데이터셋 (≈400장) — 한울 2026-05-17 마감 예정 (Sprint 2 입력 자산)

## 최근 변경
- 2026-05-16: Council (5 agents, 2 rounds) — V2_ARCHITECTURE_SPRINT1.md 신설 (14→6 컴포넌트 점진 분해)
- 2026-05-16: placeholder 값 임의 결정 (h=1.0, 거리=노트중심, 판정선 y=-2.25 등)
- 2026-05-16: spec 문서 풀세트 작성 (CHART_SPEC, JUDGMENT_SPEC, V1_SALVAGE_MAP, REFACTOR_NOTES, V2_ARCHITECTURE)
- 2026-05-16: Notion / Figma / Discord MCP 설치
- 2026-05-16: Discord 풀 크롤 (텍스트 채널 2 + 포럼 thread 11), DISCORD_LOG.md 정리

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
