# Rhyffle v2 — Spec & Planning Index

> **모든 spec 관련 작업의 진입점.**
> Spec 정본 = 로컬 markdown snapshot (`.claude/context/notion_snapshots/`). Notion 공식 워크스페이스는 직접 접근 불가 (한울 개인 워크스페이스). 갱신 필요 시 사용자가 Notion에서 Export → 본 폴더 갈아끼움.
> 추가로 사용자 개인 Notion mirror에 일부 페이지 있음 (mcp__notion__notion-search/fetch 로 접근). 현재 Rhyffle HQ 1페이지만 (5/23 시점).

---

## ⚠️ 버전 경계 (가장 중요)

### v2는 다시 두 버전 — 상호 보완 관계

| 버전 | 생성 | 핵심 변화 | 페이지 ID prefix |
|---|---|---|---|
| **v2 2차 (COSMO 통합)** | 2026-05-11 | 8 라인, 40장 덱, COSMO Objekt 카드 | `35d36a63...` |
| **v2 1차 (포커 기반)** | 2025-09-07 | 7 라인, 56장 덱 (52+4 조커), 포커 매커닉 | `26736a63...` |

두 버전이 **완전 대체 관계가 아니라 보완 관계** — v2 2차가 일부 영역만 갱신, 나머지는 v2 1차 spec이 살아있음:

| 영역 | v2 1차 (정본?) | v2 2차 (정본?) | 현재 정본 |
|---|---|---|---|
| 라인 수 | 7 | **8** | v2 2차 ★ (5/13 디코 확정) |
| 덱 크기 | 56장 (포커) | **40장** | v2 2차 ★ |
| 카드 데이터 | 포커 (4문양 13랭크) | **COSMO Objekt** | v2 2차 ★ |
| 점수 시스템 | 랭크 점수 + 족보 점수 | **클래스 점수 + 시너지 점수** | v2 2차 ★ |
| 강화 | (없음) | **화음 / 공명 (각 10단계)** | v2 2차 신규 |
| 족보 12종 + 카드군 12종 | **명시** | (언급 없음) | **불확실** — 데모 제외? 한울 확인 필요 |
| 양면 카드 / 능력 (고유+일반) | **명시** | (강화로 일부 대체?) | **불확실** |
| 반물질 (antimatter) 재화 | **명시** (양면 활성화) | (언급 없음) | **불확실** |

★ = 명확히 v2 2차로 대체된 항목  
🔴 **확인 필요**: 족보/카드군/양면/능력/반물질 등 v2 1차 매커닉이 데모에 포함되는지 한울 확인. 가설: 데모(2차) = 시너지만, 프로덕션(1차+2차) = 풀 매커닉

### v1 (절대 무시)
- v1 Notion 페이지: `(old)` 접미사 / `Brainstorming` / `용어집` / `재화` / 옛 `Roadmap` / `게임 시스템 수정` / `카드군` 옛 페이지
- v1 repo (`C:\Users\ljk91\OneDrive\문서\Github\Rhyffle`) 의 spec/주석/문서
- v1에서 가져오는 것은 **코드/세팅만**

---

## 현재 Sprint 상태

- **Sprint 1 / 1.5.x**: 완료 (5/19 Sprint 1.5.6 종료, 62/62 EditMode tests PASS)
- **Sprint 2 (카드 시스템 도입)**: 입력 자산 대기
  - 카드 데이터셋 (≈400장) — 한울+동욱 작업 중 (5/21 활발히 진행. ETA 미확정)
  - 5/21 추가 spec 변경: 아덴(ARDEN) 1차 제외 / Class Premier→Special / 파일명 규약 / 그룹별 포맷 분기 / 멤버 53명 예정 / DB 오타 한울 일괄 검수
- **이전 Sprint 정의 (260514)** = 플레이 화면 1차 구현 (Sprint 1.5.x 완료로 더 진화한 상태)

---

## 로컬 spec 인덱스 (`.claude/context/notion_snapshots/개인 페이지 & 공유된 페이지/`)

### 정본 (v2)
| 페이지 | 경로 (.claude/context/notion_snapshots/개인 페이지 & 공유된 페이지/ 이하) | 비고 |
|---|---|---|
| 게임 시스템 (v2 2차) ★ | `Rhyffle HQ/기획/게임 시스템 35d36a63929a8036b77efc41b2c5d63c.md` | 현재 정본. 5/14 한울 디코 공유 |
| 카드 시스템 (v2 2차) ★ | `Rhyffle HQ/기획/카드 시스템 35d36a63929a805cab02eae26e1e707b.md` | COSMO 데이터 모델, JSON 스키마. **5/20 한울+동욱 공동 작성** |
| 게임 시스템 (v2 1차) | `Rhyffle HQ/기획/게임 시스템 26736a63929a8070b53dd9b991958815.md` | 포커 기반 매커닉. v2 2차로 일부 대체 |
| 카드 시스템 (v2 1차) | `Rhyffle HQ/기획/카드 시스템 26736a63929a80fc95d7c507104e6772.md` | **족보 12종 + 카드군 12종 + 양면 + 능력 매커닉. v2 2차에 없어서 정본 가능성 큼** |
| 족보 시스템 밸런싱 | `Rhyffle HQ/족보 시스템 밸런싱 1ed36a63929a811ca4acfdd00fb02e9d.md` | 족보 12종 + 배수 (250703 ver). 시뮬레이션 코드 `github.com/gari0525/Rhyffle-jokbo` |

### 보조 자료
| 페이지 | 경로 (.claude/context/notion_snapshots/...) | 비고 |
|---|---|---|
| Rhyffle HQ 인덱스 | `Rhyffle HQ 1e436a63929a8012aff0e7c69c4907ff.md` | 전체 페이지 트리 목차 |
| 회의록 (Planning) | `Rhyffle HQ/회의록 1ec36a63929a80898ca5deaa6fe737bf.md` | |
| 회의록 (Design) | `Rhyffle HQ/회의록 22536a63929a80daa95cd270ff25e8e5.md` | |
| API 명세 | `Rhyffle HQ/API 명세 1e436a63929a80518e80ddf0689335ee.md` | 서버 spec |
| UI / UX | `Rhyffle HQ/UI UX 1e436a63929a80788351db556829b4c4.md` + 자녀 (`카드 관리 화면` 등) | 5/20 동욱과 공유 트리 |
| 채보 디자인 툴 | `Rhyffle HQ/채보 디자인 툴 1ec36a63929a80fea1a7dada09a1acc8.md` | 김은규 채보툴 가이드 |
| 개발 로드맵 | `Rhyffle HQ/개발 로드맵 23136a63929a80239927decaf8789c97.md` | |
| Roadmap (v2) | `Rhyffle HQ/Roadmap 1ec36a63929a80358966fe2d9c641647.md` | |
| Recruit / Members / Rhyffle 소개 / Albums / Game Sound / Font / Unity / 협업 방식 / QA | `Rhyffle HQ/` 각 `.md` 파일 | 일반 자료 |

### v1 BLACKLIST (참조 금지)
- `Rhyffle HQ/기획/게임 시스템 (old) 1ec36a63929a800fb0aed91172693990.md`
- `Rhyffle HQ/기획/게임 시스템 수정 24c36a63929a800db115c3dba7b3d8e9.md`
- `Rhyffle HQ/기획/카드 시스템 (old) 1ec36a63929a80848c6fe38867dac5b3.md`
- `Rhyffle HQ/기획/용어집 1f236a63929a801c8595e861cff5bf31.md`
- `Rhyffle HQ/기획/재화 22436a63929a803b9607c56c79f8a3a0.md`
- `Rhyffle HQ/기획/카드군 26c36a63929a802bb5e3e1e433d79f07.md` (203B 빈 페이지)
- `Rhyffle HQ/카드군 26e36a63929a8016be67d6968e8d1f3f.md` (root level, 693B)

---

## 로컬 spec 정본 (Notion 미반영)

- **채보 파일 포맷**: `.claude/context/CHART_SPEC.md` ← Sprint 1 채보 파서 입력 spec
- **채보 샘플 JSON**: `.claude/context/samples/chart_sample.json`
- 채보툴 자체 (실행 파일) = Google Drive (김은규 공유, 2026-05-11)
- **판정 정리본**: `.claude/context/JUDGMENT_SPEC.md`

---

## v1 → v2 코드 salvage

- **`.claude/context/V1_SALVAGE_MAP.md`** ← Sprint 1 기준 매핑 (9 그대로 / 7 부분 / 2 폐기 / 4 신규)
- **`.claude/context/REFACTOR_NOTES.md`** ← v1 코드 품질 이슈 (잠재 버그 6 / 매직 넘버 11 / 구조 11 / 성능 7 / dead code 8)
- **`.claude/context/V1_SCENE_INSPECTION.md`** ← v1 Game.unity 씬 인스펙터 값 (Unity MCP 추출)
- **`.claude/context/V2_ARCHITECTURE.md`** ← v2 도착점 청사진 (14 컴포넌트, 이벤트 흐름, v1→v2 매핑표)
- **`.claude/context/V2_ARCHITECTURE_SPRINT1.md`** ← **Sprint 1 진입 plan**
- **`.claude/context/CARD_FRAMEWORK_ASSUMPTIONS.md`** ← Sprint 1.5.x 카드 프레임워크 가정

---

## Figma 인덱스

- **Lo-Fi (현재 정본)**: https://www.figma.com/design/q5KsAdjcXldqWjVm2JUuYl/Lo-Fi
- Hi-Fi 프로젝트 (`597186320`): 디자이너 부재로 보류

추가 Figma 파일은 발견 시 여기에 등록.

---

## Notion 접근 방식 (5/23 갱신)

- **공식 워크스페이스 (한울 개인)**: 직접 접근 불가 — 사용자도 멤버이지만 MCP integration 부여 권한 없음
- **사용자 개인 mirror**: Notion HTTP/OAuth MCP (`https://mcp.notion.com/mcp`) 연결. 현재 `Rhyffle` 페이지 1개만 (복붙 시 trash 자동 처리 이슈로 mirror 시도 폐기)
- **현재 정본**: 공식 Notion에서 Export Markdown → `.claude/context/notion_snapshots/` 갈아끼우는 방식
- 갱신 절차:
  1. 공식 Notion에서 spec 변경
  2. 사용자가 Notion → ⋯ → Export (Markdown & CSV, Everything 포함)
  3. zip 받아서 `.claude/context/notion_snapshots/` 안에 풀기
  4. Claude에게 변경사항 알려주면 SPEC.md 인덱스 + 영향받는 파일 갱신

---

## Spec 변경 절차

1. **Notion 원본 페이지에서 먼저 수정** (정본은 공식 Notion)
2. 변경 페이지 Export → `.claude/context/notion_snapshots/` 갱신
3. 이 파일 인덱스 갱신 (페이지 추가/이동/삭제 등 구조 변경 시)
4. 큰 변경은 `.claude/context/DECISIONS.md` 에 결정 기록

---

## 관련 파일

- **`.claude/context/CURRENT_STATUS.md`** — 작업 포인터 (현재 단계 / 다음 액션 / 블로커). 새 세션 진입 시 최우선
- `.claude/context/RHYFFLE_CONTEXT.md` — 작업 방식 / 행동 원칙 (HOW)
- `.claude/context/DISCORD_LOG.md` — Discord/팀 메시지 raw input
- `.claude/context/DECISIONS.md` — 결정 로그 (WHY)
- `.claude/context/GLOSSARY.md` — 도메인 용어 (WORDS)
- `.claude/context/TEAM.md` — 팀 멤버 & Discord 매핑
- `CLAUDE.md` (루트) — 프로젝트 최상위 메모
