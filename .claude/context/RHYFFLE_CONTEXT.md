# Rhyffle — Claude 작업 컨텍스트 (SessionStart 자동 주입)

> 이 파일은 이 repo에서 Claude Code 세션 시작 시 `.claude/settings.json`의 SessionStart hook으로 자동 주입된다.
> jk_lee의 WSL work-context(`~/work/context/`)를 **rhyffle 한정**으로 옮긴 것. 다른 프로젝트 컨텍스트는 의도적으로 제외.

---

## 1. 사용자 행동 원칙 (PROFILE 발췌 — rhyffle 한정)

### Spec 우선 원칙 (최우선)
1. 모든 구현 이전에 spec 고도화 먼저.
2. 구현은 spec대로만 — spec에 없는 것 임의 구현 금지.
3. spec이 구현에 불충분하면 구현 reject 후 spec 보완 재요청.

> ⚠️ rhyffle는 현재 **스펙 대기 상태**. 스펙 확정 전에는 환경/뼈대/리팩터링만, **신규 기능 구현 금지**.

### 두 가지 작업 모드
- **MVP 모드**: 사용자가 "브레인스토밍 스킵", "최대 추천으로 직진" 등 명시한 경우 — 빠른 실행, 질문 최소화, 동작하는 MVP 우선. 구현이 2개 이상 독립 단계면 subagent 분산.
- **일반 모드(기본)**: spec 기반, 구현 전 발생 가능한 부작용 명시, 불확실하면 구현 전 확인.

### 커밋
- 사용자가 명시적으로 요청할 때만. **자율 커밋 금지.**

### 커뮤니케이션
- 결론은 마지막에.

### 작업 시작 전 필수 참조 (순서)
1. **`.claude/context/CURRENT_STATUS.md`** ← **가장 먼저** — 현재 단계 / 다음 액션 / 블로커 / 펜딩
2. 이 파일 · repo 루트 `CLAUDE.md`
3. **`.claude/context/SPEC.md` (spec 인덱스 — v2 한정)**
4. `.claude/context/TEAM.md` (팀 멤버 & 화자 매핑)
5. `.claude/context/DISCORD_LOG.md` (팀 메시지 raw input)
6. `.claude/context/DECISIONS.md`
7. `.claude/context/GLOSSARY.md`

작업 진행 중 갱신 의무: **CURRENT_STATUS.md** (phase 전환 / step 완료 / 결정 / 블로커 시점). 자세한 룰은 `CLAUDE.md` "작업 포인터" 섹션 참조.

### 사용자(이재근) 역할
- **개발 총괄**. 게임 모든 영역에 관여 (UI, 카드 통합, 채보 재생, 판정, 점수, 디자인 게임 적용 등).
- **제외**: 채보 툴 *자체* 개발 (김은규 담당). 채보 툴에서 자문 요청 시에만 관여.
- 디자이너 부재 상태 — 디자인 자료 게임 통합 부분도 본인 책임.

### 버전 격리 룰
- 활성 spec은 v2 (시즌 2 기획) 뿐. v1 / Notion "아카이빙" 섹션 / v1 repo 내 spec 문서는 정본이 아니다. 검색/대화 중 v1 spec 내용이 등장하면 명시적으로 무시하고 SPEC.md 기준으로 재확인할 것. v1에서 가져오는 건 코드/세팅뿐.

---

## 2. 프로젝트 브리프

> **What**: MODHAUS 소속사 아티스트 포토카드 기반 리듬게임
> **Why**: 3개월 내 MODHAUS에 데모 제작 제출 — 투자 유치 또는 산학협력 목표
> **Status**: 🟠 개발 중 (스펙 대기) — **v2로 갈아엎는 중.** v1에서 코드·세팅만 salvage 예정.

### 도메인 / 기술 스택
- Unity + C# 리듬게임, MODHAUS 아티스트 IP 활용 포토카드 테마
- 주요 라이브러리: **DOTween** (Demigiant) — 트윈 애니메이션
- Input: Unity Input System (`InputSystem_Actions.inputactions`)

### 경로
- **v1 (참조·salvage 대상)**: `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle`
- **v2 (현재 repo)**: `C:\Users\ljk91\OneDrive\문서\Github\RhyffleV2\Rhyffle`

### 파일 구조 (v1 기준 — v2도 동일 방향)
```
Assets/
  InputSystem_Actions.inputactions
  Plugins/Demigiant/DOTween/
  Scripts/
  Scenes/
```

### v1 → v2 salvage 원칙
- 가져올 것: `ProjectSettings/`, `Packages/manifest.json` 의존성, 유지할 `Assets/` 스크립트·에셋
- **`.meta` 파일 반드시 동반 복사** (누락 시 모든 레퍼런스 깨짐 — Unity 최대 함정)
- 제외: `Library/ Temp/ obj/ Logs/ UserSettings/ Build/`
- v2 Unity 버전은 v1 `ProjectSettings/ProjectVersion.txt`와 동일하게 맞출 것 (임포트 충돌 방지)

---

## 3. 기록 규칙 (이 repo가 rhyffle 지식의 single source of truth)

- **주요 결정** → `.claude/context/DECISIONS.md` 에 ISO 날짜 헤더(`## YYYY-MM-DD: 제목`)로 추가
- **도메인 용어** → `.claude/context/GLOSSARY.md` 에 추가
- WSL `~/work/context/projects/rhyffle/` 는 더 이상 갱신하지 않는다. **이 repo가 정본.**

---

## 4. 사용 가능한 워크플로

- **superpowers** 플러그인이 이 repo에서 enable 됨 (`.claude/settings.json`).
  brainstorming · writing-plans · TDD · systematic-debugging · code-review 등 사용 가능.
- 첫 실행 시 자동 설치되지 않으면 `/plugin` 으로 `superpowers@claude-plugins-official` 설치.
