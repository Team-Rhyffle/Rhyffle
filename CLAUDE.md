# Rhyffle v2 — CLAUDE.md

MODHAUS 소속 아티스트 포토카드 기반 리듬게임 (Unity / C#). v2 리빌드 중.

## 환경

이 repo는 jk_lee의 WSL Claude 작업환경을 **rhyffle 한정**으로 옮긴 것이다.

- 세션 시작 시 `.claude/context/RHYFFLE_CONTEXT.md` 가 SessionStart hook으로 **자동 주입**된다 (행동 원칙 + 프로젝트 브리프 전체).
- **spec 인덱스: `.claude/context/SPEC.md`** — 모든 spec 관련 작업 진입점. Notion / Figma / Sprint / v1 salvage 참조처가 여기 모여있다. v2 spec 관련 질문/작업 시 **항상 먼저 읽을 것.**
- 결정 기록: `.claude/context/DECISIONS.md` · 용어집: `.claude/context/GLOSSARY.md` — **이 repo가 rhyffle 지식 정본** (WSL work-context는 더 이상 갱신 안 함).
- superpowers 플러그인이 이 repo에서 enable 됨 (brainstorming/plan/TDD/debug 등).

## 버전 격리 (v1 ↔ v2)

- **활성 spec은 v2 (시즌 2 기획) 뿐.** v1 spec / Notion "아카이빙" 섹션 / v1 repo 내 spec 문서는 정본 아님.
- v1에서 가져오는 것은 **코드/세팅만**. spec 내용은 절대 v1에서 가져오지 않는다.
- 검색/대화 중 v1 spec 내용이 등장하면 명시적으로 무시하고 SPEC.md 기준으로 재확인.

## 디코 / 팀 메시지 자동 기록

다음 트리거 발생 시 Claude는 메시지를 검토 후 **기획/일정 관련 부분만** `.claude/context/DISCORD_LOG.md` 에 append하고 사용자에게 한 줄 보고한다:

- 사용자 메시지에 **"디코 메세지"** 또는 **"디코"** 가 명시된 경우
- 메시지에 **김한울** 또는 **김은규** 의 발언이 인용된 경우 (이름 옆 콜론·따옴표 등 형식 무관)

### 콘텐츠 필터 (중요)
저장 대상은 **기획 또는 일정 관련 내용만**. 나머지는 폐기.

- ✅ 저장: 게임 시스템/카드/UI/음원/채보/빌드 등 spec 관련 결정·제안·질문, 회의 일정, 마일스톤, 데드라인, 작업 분배, sprint 범위 변경, 외부 일정 통보 (휴가/부재 등 작업에 영향)
- ❌ 폐기: 잡담, 농담, 안부, 감정 표현, 이모지 도배, 게임/문화 잡담, spec과 무관한 개인 의견 등

판단 애매하면 **저장 쪽으로 기울임** + 사용자에게 "이거 기록할까?" 한 번 확인. 잡담 명백하면 보고에만 한 줄 "잡담이라 미기록" 표시하고 넘김.

기록 항목: 날짜 / 발화자 / 발언 원문(요약 X, 원문 유지) / 맥락 / spec 영향 / 상태(pending|반영완료|폐기).
spec 영향이 큰 내용이면 사용자에게 "Notion 페이지에도 반영하시겠어요?" 확인. **Notion이 정식 spec, 이 로그는 raw input.**

## 핵심 원칙 (전체는 RHYFFLE_CONTEXT.md)

- **Spec 우선 (최우선)**: 스펙 확정 전 신규 기능 구현 금지. rhyffle는 현재 **스펙 대기** 상태 — 지금은 환경·뼈대·salvage만.
- 커밋은 사용자가 명시 요청할 때만. **자율 커밋 금지.**
- 결론은 마지막에.

## 작업 포인터 (CURRENT_STATUS.md)

- 새 세션 시작 시 `.claude/context/CURRENT_STATUS.md` **가장 먼저 읽기** — 현재 단계 / 다음 액션 / 블로커 / 펜딩 한 곳에 모음
- 다음 시점에 즉시 갱신 (한 두 줄 수준):
  - Phase 전환 (spec → 구현, Sprint 종료 등)
  - Week step 완료 / 새 step 시작
  - 계획 변경 / 결정
  - 블로커 발생 / 해소
  - 일시 중지 / 핸드오프
- 갱신 가이드: "다음 액션" 은 구체적(파일명/함수명/step 번호)으로. 자세한 결정 근거는 `DECISIONS.md`. "최근 변경" 은 최신 5개만 유지

## v1 → v2

- v1 (salvage 대상): `C:\Users\ljk91\OneDrive\문서\Github\Rhyffle`
- v2 (현재): `C:\Users\ljk91\OneDrive\문서\Github\RhyffleV2\Rhyffle`
- salvage 시 `.meta` 동반 복사 필수, `Library/Temp/obj/Logs/UserSettings/Build` 제외, Unity 버전 v1과 일치.
