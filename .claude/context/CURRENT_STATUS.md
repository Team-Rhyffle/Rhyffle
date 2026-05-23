# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`.

---

## 현재 단계
**Sprint 1.5.6 완료** — DeckSystem + Graveyard. **62/62 EditMode tests PASS**. Sprint 2 대기 (김한울 데이터셋).

- Sprint 1 (2026-05-18 완료): 14/14 task + Steelman 게이트 3개 PASS.
- Sprint 1.5 (2026-05-19 완료): 10/10 task DONE. 36/36 EditMode tests PASS.
- Sprint 1.5.1 (2026-05-19 완료): 5/5 task DONE (Lane Visualization + Card-Lane Mapping). 40/40 tests PASS.
- Sprint 1.5.2 (2026-05-19 완료): 5/5 task DONE. 58/58 tests PASS.
- Sprint 1.5.3 (2026-05-19 완료): 5/5 task DONE. HUD 재배치 + 5 placeholder + 3 영역 visual band. 58/58 tests PASS.
- Sprint 1.5.4 (2026-05-19 완료): NotoSansKR SDF (Apache 2.0) + atlas sub-asset 저장. 58/58 tests PASS.
- Sprint 1.5.5 (2026-05-19 완료): PauseButton wire + code polish + DECISIONS.md 정리.
- Sprint 1.5.6 (2026-05-19 완료): DeckSystem.cs + SceneBootstrap wiring + 통합 검증. 62/62 PASS.

## 다음 액션
**Sprint 2 진입** — 김한울 카드 데이터셋 도착 시 시작 (JSON loader 교체, SeedDummyDeck 제거).

## 블로커
**Sprint 2 진입 블로커**: 카드 데이터셋 입력 진행 중 (5/17 마감 → 4일 초과, 5/21 기준 한울+giantdaegari 협업으로 활발히 작업 중). **완성 시점 미정** — 한울에게 ETA 확인 필요.

## 펜딩 (사용자 / 외부)
- 카드 데이터셋 (≈400장) — 5/21 입력 진행 중. **ETA 확인 필요**
- 신규 기획 문서 (Notion `35d36a63929a805c...`, 한울+동욱 공동 작성, 5/19~) — **Notion MCP integration 미공유로 접근 불가**. 사용자가 공유하거나 요약 전달 필요
- A-high straight (10-J-Q-K-A) spec — Sprint 2 시 확정
- 묘지/덱/키노트/일반능력 spec — Sprint 2 입력 대기
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**: JSON 채보 드롭 검증 끝난 뒤
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- `giantdaegari` ↔ 이동욱 매핑 확정 (강한 추정, TEAM.md 보완 필요)
- 곡 정보 데이터 wire (chartName + audio 메타 spec — 펜딩)
- 셔플 기능 (Sprint 2 카드 spec 도착 시)
- 덱묘지 데이터 표시 (Sprint 2)
- Figma Hi-Fi 외관 교체 (placeholder → 실제 art)

## Sprint 2 입력 자산 — 신규 확정 사항 (5/21 디코)
- **아덴(ARDEN) 유닛 1차 데이터셋 제외** — 후속 추가 여지
- **Class 명칭: Premier → Special** (기존 spec 갱신 필요)
- **카드 이미지 파일명 규약**: `<그룹>_<시즌>_<멤버>_<시리얼>` (예: `ARTMS_Divine01_Heejin_120Z`)
- **이미지 경로는 클라가 prefix** (DB는 파일명만 보유)
- **그룹별 포맷 분기**: tripleS/ARTMS 동일 enum, idntt 별도 enum (Season 종류 / Class 명칭 차이)
- **멤버 규모**: 현재 44명 + 9명 추가 예정 = 53명

## 최근 변경
- 2026-05-21: **디코 spec 변경 다수 캐치업** — 아덴 스킵 / Premier→Special / 카드 파일명 규약 / 그룹별 포맷 분기 / 멤버 53명 예정 (DISCORD_LOG.md 2026-05-21 항목)
- 2026-05-20: **새 기획 문서 공유** (한울+동욱 공동, Notion `35d36a63929a805c...`) — 접근 권한 필요
- 2026-05-19: **Sprint 1.5.6 T3 완료** + Phase push — DeckSystem 통합 검증 (키노트 → 필드 교체 + 묘지 합쳐 셔플). 62/62 PASS.
- 2026-05-19: **Sprint 1.5.6 T2 완료** (c442ce9) — SceneBootstrap EnsureCardSystemAndBoard에 DeckSystem 생성 + cardSystem wire.
- 2026-05-19: **Sprint 1.5.6 T1 완료** (c1b0a5c) — DeckSystem.cs 신규 (단일 묘지, Fisher-Yates, KeyNote trigger) + 4 tests.

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
