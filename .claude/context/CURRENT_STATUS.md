# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`, spec 인덱스는 `SPEC.md`.

---

## 현재 단계
**Sprint 1.5.6 완료** — DeckSystem + Graveyard. **62/62 EditMode tests PASS** (5/19).  
**Sprint 2 진입 대기** — 카드 데이터셋 입력 활발히 진행 중 (5/21~). ETA 미확정.

### 완료된 Sprint 요약
- Sprint 1 (2026-05-18 완료): 14/14 task + Steelman 게이트 3개 PASS
- Sprint 1.5 (2026-05-19 완료): 10/10 task. 36/36 tests PASS
- Sprint 1.5.1 (2026-05-19 완료): Lane Visualization + Card-Lane Mapping. 40/40 tests PASS
- Sprint 1.5.2 (2026-05-19 완료): 58/58 tests PASS
- Sprint 1.5.3 (2026-05-19 완료): HUD 재배치 + 5 placeholder + 3 영역 visual band. 58/58 tests PASS
- Sprint 1.5.4 (2026-05-19 완료): NotoSansKR SDF (Apache 2.0) + atlas sub-asset. 58/58 tests PASS
- Sprint 1.5.5 (2026-05-19 완료): PauseButton wire + code polish + DECISIONS.md 정리
- Sprint 1.5.6 (2026-05-19 완료): DeckSystem.cs + SceneBootstrap wiring + 통합 검증. 62/62 PASS

## 다음 액션

### 즉시 가능 (외부 입력 없이)
1. **DISCORD_LOG.md 미반영 spec 확정 사항 한울에게 확인 요청** (특히 Class Premier→Special 페이지 갱신 / 데모 spec 범위 — 포커 매커닉 + 카드군 포함 여부)
2. **Sprint 2 prep work 시작 가능** (spec 도착 전이라도 가능한 작업):
   - 카드 데이터 모델 C# 정의 (`CardMetadata.cs`) — JSON 스키마 그대로 매핑 (`artist/member/season/collectionNo/class/imageFile`)
   - JSON deck loader 인터페이스 설계 (`SeedDummyDeck` 교체 자리)
   - 화음/공명 강화 시스템 구조 검토 (Sprint 2 범위 여부 한울 확인 후)

### 외부 입력 대기
- 카드 데이터셋 ETA — 한울 (5/21 입력 진행 시작, 5/22~5/23 활동 확인 안 됨)
- 등급 단계 (4단계 vs 5단계) 정본 확정 — 한울
- A-high straight (10-J-Q-K-A) spec — 한울
- 묘지/덱/키노트/일반능력 spec 추가 디테일 — Sprint 2 spec 도착 시
- Premier → Special 노션 페이지 반영 여부 — 한울

## 블로커
- **Sprint 2 본격 진입 블로커**: 카드 데이터셋 입력 진행 중 (5/17 마감 → 4일 초과, 5/21 활발히 작업 진행). 5/22~5/23 진행 확인 안 됨 → ETA 확인 필요
- **spec 불확실성**: v2 2차에 누락된 영역(족보/카드군/양면/능력/반물질)이 데모에 포함되는지 미확정 → 한울 확인 필요 (Sprint 2 spec 도착 시 함께 처리 가능)

## 펜딩 (사용자 / 외부)
- 카드 데이터셋 (≈400장) — 5/21 입력 진행 중. **ETA 확인 필요**
- A-high straight (10-J-Q-K-A) spec — Sprint 2 시 확정
- 묘지/덱/키노트/일반능력 spec 추가 디테일 — Sprint 2 입력 대기
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 곡 정보 데이터 wire (chartName + audio 메타 spec — 펜딩)
- 셔플 기능 (Sprint 2 카드 spec 도착 시)
- 덱묘지 데이터 표시 (Sprint 2)
- Figma Hi-Fi 외관 교체 (placeholder → 실제 art) — 디자이너 부재 동안 보류

## Sprint 2 입력 자산 — 신규 확정 사항 (5/21 디코)
- **아덴(ARDEN) 유닛 1차 데이터셋 제외** — 후속 추가 여지
- **Class 명칭: Premier → Special** (노션 페이지 미반영, 한울 액션 필요)
- **카드 이미지 파일명 규약**: `<그룹>_<시즌>_<멤버>_<시리얼>` (예: `ARTMS_Divine01_Heejin_120Z.jpg`)
- **이미지 경로는 클라가 prefix** (DB는 파일명만 보유)
- **그룹별 포맷 분기**: tripleS/ARTMS 동일 enum, idntt 별도 enum (Season 종류 / Class 명칭 차이)
- **멤버 규모**: 현재 44명 + 9명 추가 예정 = 53명
- **DB 오타는 한울 일괄 검수** → 클라 검증 strict 가능

## 카드 spec 정본 — 1차/2차 보완 (5/23 분석)
| 영역 | 정본 |
|---|---|
| 라인 수 / 덱 크기 / 카드 데이터 모델 / 점수 식 / 강화 (화음/공명) | **v2 2차** (35d36a63) |
| 족보 12종 / 카드군 12종 / 양면 카드 / 고유능력+일반능력 / 반물질 재화 | **v2 1차 추정** (26736a63) — 데모 포함 여부 한울 확인 필요 |

## 최근 변경
- 2026-05-23: **Notion mirror 폐기 → Markdown Export 정본 전환**. `.claude/context/notion_snapshots/` 에 전체 페이지 트리 markdown 확보. SPEC.md / GLOSSARY.md / DECISIONS.md / CURRENT_STATUS.md / TEAM.md 전면 갱신
- 2026-05-23: **Notion MCP HTTP/OAuth 전환** (npx 토큰 방식 → `https://mcp.notion.com/mcp`)
- 2026-05-23: **giantdaegari = 이동욱 매핑 확정** (GitHub `gari0525/Rhyffle-jokbo` 시뮬레이션 코드 증거)
- 2026-05-23: **v2 spec 두 버전 발견 + 보완 관계 분석** (포커 1차 / COSMO 2차)
- 2026-05-21: 디코 spec 변경 다수 캐치업 — 아덴 스킵 / Premier→Special / 파일명 규약 / 그룹별 포맷 분기 / 멤버 53명 예정
- 2026-05-19: **Sprint 1.5.6 완료** + Phase push — DeckSystem 통합 검증. 62/62 PASS

## 작업 재개 시 첫 행동
1. **이 파일 (CURRENT_STATUS.md) 먼저 읽기**
2. RHYFFLE_CONTEXT.md auto-inject 확인 (SessionStart hook)
3. SPEC.md 인덱스 확인 — Notion → 로컬 markdown 경로 전환됨
4. 위 "다음 액션" 으로 즉시 진행
5. 작업 진행 / 결정 / 블로커 발생 시 이 파일 갱신

## 갱신 가이드라인
- **한 두 줄 수준**으로 압축. 긴 설명은 DECISIONS.md
- "다음 액션" 은 항상 *구체적*으로: 파일명 / 함수명 / step 번호
- "최근 변경" 은 최신 5개만 유지. 오래된 건 DECISIONS.md 로 흘려보냄
- v2 코드 진행도: 파일 단위 (예: "GameConfig.cs 완료, ChartLoader 진행 중")
