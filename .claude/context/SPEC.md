# Rhyffle v2 — Spec & Planning Index

> **이 파일은 모든 spec 관련 작업의 진입점.**
> 세부 내용은 Notion에 위임. 여기는 인덱스만.
> Claude는 v2 spec 관련 질문/작업 시 이 파일을 먼저 읽고, 필요한 페이지로 navigate.

---

## ⚠️ 버전 경계 (가장 중요)

- ✅ **활성 spec**: v2 (시즌 2 기획) — 아래 Notion 인덱스의 페이지들만
- ❌ **무시 대상**:
  - v1 Notion 페이지 (특히 "Rhyffle (아카이빙)" 섹션 — Brainstorming / 게임 시스템 (old) / 카드 시스템 (old) / 카드군 / 용어집 / 재화 / Roadmap (구) / 게임 시스템 수정 등)
  - v1 repo 안의 spec/주석/문서 (`C:\Users\ljk91\OneDrive\문서\Github\Rhyffle`)
- 채팅이나 검색 결과에 **v1 spec 내용이 등장하면 명시적으로 거부**하고 v2 기준으로 재확인할 것
- v1에서 가져오는 것은 **코드/세팅만**, spec은 절대 v1에서 가져오지 않는다

---

## 현재 Sprint

- **260514 Sprint** — 플레이 화면 1차 구현
  - 노션: https://www.notion.so/362a95ab77ed81cc8082edb0f05f51c1
  - 범위: 채보 로드 + 재생 / 노트 4종 + 판정 / Plain Mode 점수 + 콤보 / Figma Lo-Fi 목업 최소 배치
  - **제외**: 카드 시스템 전부 (드로우/필드/묘지/셔플), 시너지 점수, 족보 판정, Challenge Mode
  - Figma Lo-Fi: https://www.figma.com/design/q5KsAdjcXldqWjVm2JUuYl/Lo-Fi

---

## Notion 인덱스 (v2 한정)

### 기획 (Planning > 기획 > "Rhyffle 시즌 2")
- **게임 시스템** — https://www.notion.so/362a95ab77ed818ba887c5373786b1c8
  - 8 라인 + 슬롯 UI / 노트 4종 (탭·홀드·슬라이드·플릭 상하좌우) / 판정 4단계 / 점수 (Plain + Challenge) / 카드 구성 / 덱 / 난이도
- **카드 시스템** — https://www.notion.so/362a95ab77ed819f8b20e3dae251e614
  - 4문양 13랭크 / 등급 / 양면 카드 / 포커 족보 + 신규 4종 (쓰리페어, 레인보우, 더블트리플, 럭키세븐) / 능력 (고유+일반) / 카드군 12종 컨셉

### Roadmap
- **Roadmap 페이지** — https://www.notion.so/362a95ab77ed8168bdeae8d91433bea7
- **260514 Sprint** — https://www.notion.so/362a95ab77ed81cc8082edb0f05f51c1

### 채보 (Notion 미반영 — 로컬 spec 정본)
- **채보 파일 포맷**: `.claude/context/CHART_SPEC.md` ← Sprint 1 채보 파서 입력 spec
- **채보 샘플 JSON**: `.claude/context/samples/chart_sample.json`
- 채보툴 자체 (실행 파일) = Google Drive (김은규 공유, 2026-05-11)

### 판정 (Notion 정본, 로컬 정리본)
- **`.claude/context/JUDGMENT_SPEC.md`** ← Notion 판정 spec 추출 + 미확정 항목 트래킹

### v1 → v2 코드 salvage
- **`.claude/context/V1_SALVAGE_MAP.md`** ← Sprint 1 기준 매핑 (9 그대로 / 7 부분 / 2 폐기 / 4 신규)
- **`.claude/context/REFACTOR_NOTES.md`** ← v1 코드 품질 이슈 (잠재 버그 6 / 매직 넘버 11 / 구조 11 / 성능 7 / dead code 8)
- **`.claude/context/V2_ARCHITECTURE.md`** ← v2 도착점 청사진 (14 컴포넌트, 이벤트 흐름, v1→v2 매핑표)
- **`.claude/context/V2_ARCHITECTURE_SPRINT1.md`** ← **Sprint 1 진입 plan** (council 결정 — 6 컴포넌트 점진 분해, Week 1/2/Sprint 2 entry, placeholder 값)

### 진입점
- **Rhyffle HQ (워크스페이스 루트)** — https://www.notion.so/1e6a95ab77ed801aafadfd46f345791e

### 의도적 제외 (참고만, 정본 아님)
- `Brainstorming`, `게임 시스템 (old)`, `카드 시스템 (old)`, `카드 시스템 수정`, `게임 시스템 수정`, `카드군 (v1)`, `용어집 (v1)`, `재화 (v1)`, `Roadmap (v1)`
- 이들은 v1 유산. spec 참조 금지. 역사적 맥락 필요 시에만 사용자가 명시적으로 지시.

---

## Figma 인덱스

- **Lo-Fi (Sprint 260514 1차 구현 mockup)**
  - URL: https://www.figma.com/design/q5KsAdjcXldqWjVm2JUuYl/Lo-Fi
  - 접근: Figma 데스크탑 앱에서 열고 프레임 선택 → MCP로 spec 추출

추가 Figma 파일은 발견 시 여기에 등록.

---

## v1 Salvage 참조 (코드만)

- **v1 repo**: `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle`
- **Unity 버전**: 6000.2.8f1 (v2와 일치 확인됨)
- ⚠️ **v1 spec 내용 절대 사용 금지**. 코드 구조/유틸/세팅만 salvage
- `.meta` 파일 동반 복사 필수 (누락 시 Unity 레퍼런스 깨짐)
- 제외 디렉토리: `Library/ Temp/ obj/ Logs/ UserSettings/ Build/`
- 상세 salvage 매핑: `.claude/context/V1_SALVAGE_MAP.md`

---

## Spec 변경 절차

1. **Notion 원본 페이지에서 먼저 수정** (정본은 Notion)
2. 이 파일 인덱스 갱신 (페이지 추가/이동/삭제 등 구조 변경 시)
3. 큰 변경은 `.claude/context/DECISIONS.md` 에 결정 기록

---

## 관련 파일

- **`.claude/context/CURRENT_STATUS.md`** — 작업 포인터 (현재 단계 / 다음 액션 / 블로커). 새 세션 진입 시 최우선
- `.claude/context/RHYFFLE_CONTEXT.md` — 작업 방식 / 행동 원칙 (HOW)
- `.claude/context/DISCORD_LOG.md` — Discord/팀 메시지 raw input (Notion 갱신 전 임시 spec 소스)
- `.claude/context/DECISIONS.md` — 결정 로그 (WHY)
- `.claude/context/GLOSSARY.md` — 도메인 용어 (WORDS)
- `CLAUDE.md` (루트) — 프로젝트 최상위 메모
