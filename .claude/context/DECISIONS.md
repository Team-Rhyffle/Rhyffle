# Rhyffle — 결정 기록

> rhyffle 지식 정본. WSL work-context가 아니라 이 파일이 source of truth.
> 형식: `## YYYY-MM-DD: 제목` / Context / Decision / Outcome

---

## 2026-05-16: v2 리빌드 + Windows Claude 환경 분리

- **Context**: rhyffle를 v2로 갈아엎기로 함. Unity Editor는 Windows 호스트 네이티브에서 돌려야 하므로(WSL/리듬게임 타이밍 부적합), 게임 작업을 Windows PowerShell의 별도 Claude Code 인스턴스로 분리. 기존 WSL Claude는 그대로 유지.
- **Decision**:
  - v2는 새 repo (`...\RhyffleV2\Rhyffle`), v1(`...\Github\Rhyffle`)에서 코드·ProjectSettings만 salvage.
  - WSL work-context를 통째로 옮기지 않고 **rhyffle 한정**으로만 이 repo `.claude/context/` 에 이관. 이 repo가 rhyffle 지식 정본.
  - SessionStart hook은 PowerShell 기반(bash/python 의존 제거), project-scoped — Windows 글로벌 `.claude` 미변경.
  - superpowers 플러그인 repo-scoped enable.
- **Outcome**: `.claude/{context/RHYFFLE_CONTEXT.md, context/DECISIONS.md, context/GLOSSARY.md, settings.json}` + 루트 `CLAUDE.md` 생성. Windows Claude를 repo에서 실행 시 자동 로드.

---

## 2026-05-16: Spec 인덱스 도입 + v1/v2 격리 룰

- **Context**: v2 리빌드 진행 중. Notion에 v1 유산 페이지("Rhyffle (아카이빙)" 섹션)가 여전히 검색됨. v1 repo 내부 spec 문서/주석도 마찬가지. Notion MCP / 검색 시 v1 spec이 v2 작업에 섞일 위험.
- **Decision**:
  - `.claude/context/SPEC.md` 신규 — spec 작업 단일 진입점. Notion 인덱스, Figma URL, 현재 sprint, v1 salvage 참조처를 한 곳에. 세부 내용은 Notion에 위임 (이 파일은 인덱스만).
  - 버전 격리 룰 명문화: 활성은 v2뿐. v1/아카이빙 페이지는 spec 정본 아님. v1에서 가져오는 건 코드/세팅만, spec 내용 금지.
  - `CLAUDE.md` + `RHYFFLE_CONTEXT.md` 에 SPEC.md 항상 참조 + 버전 격리 룰 등재.
- **Outcome**: `.claude/context/SPEC.md` 생성. `CLAUDE.md` · `RHYFFLE_CONTEXT.md` · `DECISIONS.md` 동기 업데이트. 향후 spec 변경은 Notion → SPEC.md 인덱스 갱신 → 큰 변경은 DECISIONS.md 기록 순.

---

## 2026-05-16: 디코/팀 메시지 자동 기록 룰

- **Context**: spec 소스가 Notion만이 아니다. 김한울/김은규의 디코 발언이 종종 기획에 직접 반영되는 raw input. 휘발성 높음 + Claude가 매번 사용자에게 다시 물어야 하는 마찰 발생.
- **Decision**:
  - `.claude/context/DISCORD_LOG.md` 신규 — 날짜순 append-only 로그.
  - 트리거: 사용자 메시지에 "디코" 명시 OR 김한울/김은규 발언 인용 시 즉시 append + 한 줄 보고.
  - 위계: Notion = 정식 spec, DISCORD_LOG.md = raw input. spec 영향이 크면 Notion 반영 여부 사용자에게 확인.
  - `CLAUDE.md` · `RHYFFLE_CONTEXT.md` · `SPEC.md` 동기 등록.
- **Outcome**: DISCORD_LOG.md 템플릿 생성. 향후 디코 내용은 여기에 누적 → 필요 시 Notion 정식 페이지로 승격.

---

## 2026-05-16: Sprint 1 진입 아키텍처 — 점진 분해 + placeholder 임의 결정

- **Context**: V2_ARCHITECTURE.md (14 컴포넌트 청사진) 가 학생 1인 Sprint 1 진입점으로 과설계 우려 → council 소집 (5 agents, 2 rounds: Steelman / Red Team / Context Keeper / Pragmatist / Constraint Challenger).
  - Pragmatist: 14개 일제 = 35~60h vs 6개 점진 = 20~35h (1주차 동작 보장 차이)
  - Constraint Challenger: Sprint 데드라인은 외부 압박 약함, 협업 통로 열려있음
  - Red Team: 14개에서 이벤트 폭포 디버그 지옥 + JudgmentResult 슬림이 카드 시너지 도입 시 시그니처 변경 필수
  - Context Keeper: v2 코드 0줄 = 매몰비용 없음. 노동시간 출처 없는 추정
  - Steelman: 분해 방향성 자체는 옳음, "어디서 멈췄는가" 가 핵심
- **Decision**:
  - V2_ARCHITECTURE.md = **도착점**으로 유지 (삭제 X)
  - Sprint 1 진입점 = **6개 컴포넌트** (GameConfig / Conductor / ChartLoader / Note / JudgeProcessor / GameLoop) 신규 문서 `V2_ARCHITECTURE_SPRINT1.md` 작성
  - Sprint 2 진입 직전 분리 작업 (작동 코드 IDE refactor): NotePool, NoteSpawner, NoteDropper, InputRouter, ScoreSystem, ComboCounter, HUDController, C# event 시스템
  - JudgmentResult reserve context field → Sprint 2 entry 시점 추가 (카드 spec 미정인 지금 추측 회피)
  - 미확정 spec 3건 (h, 거리 기준점, 곡 메타) → 한울 질의 대기 없이 **placeholder 임의 결정으로 시작**. 향후 사용자 갱신:
    - `h` = 1.0 (Unity world unit)
    - 거리 기준점 = 노트 중심
    - BPM = 120, Offset = 0 (1곡 하드코드)
    - 판정선 y = -2.25, 출발선 y = 7.75 (v1 참고치)
    - playerSpeed (r) = 1.0
- **Outcome**:
  - `V2_ARCHITECTURE_SPRINT1.md` 신설 — Week 1 / Week 2 / Sprint 2 entry 분해
  - DoD 명시 + Sprint 1 미도입 영역 명시 (풀링/이벤트/외부 분리 컴포넌트들)
  - 시작 전 한울 spec 질의 → cancelled (placeholder 결정으로 차단 요인 자체 제거)
  - 다음 작업: Sprint 1 implementation 시작 가능 상태

---

## 2026-05-17: Sprint 1 spec 잔여 빈칸 일괄 확정 + JSON 검증 우선

- **Context**: 한울이 디코에서 "채보툴 / 플레이화면 1차 구현 기획안 리뷰 두 트랙 공유"를 요청 (5/17 10:32). 사용자(이재근) 본인이 spec 결정 권한을 충분히 가지고 있어 한울 답변 대기 대신 직접 결정으로 spec 잔여 빈칸을 일괄 해소. 동시에 데모 음원/곡 메타 준비보다 JSON 채보 드롭 시각 검증이 선행되어야 한다는 우선순위 합의.
- **Decision**:
  - `h` = 1.0 유지 (Unity world unit)
  - 라인 1개 가로 폭 = 화면 fit 임의 (24 lane이 카메라 가시 영역 가로폭 채움). `GameConfig.LANE_WIDTH`
  - 배속 r 사용자 옵션 노출 X (Sprint 1)
  - 플릭 시간창 없음 — 거리 조건만으로 판정
  - HoldNote SpMiss = **v1과 완벽히 동일한 로직** (마지막 터치 떼는 시점 등급 분류, `JudgmentGrade.SpMiss` enum 유지)
  - 점수 UI = v1 `GameScoreInfo.cs` 참고 (표시 형식 salvage)
  - 콤보 UI = 중앙 상단 (placeholder 네모상자 OK)
  - Pause / Resume UX = v1 `GamePlayer.cs` 참고
  - 곡 메타 포맷 = 이재근 작성, **JSON 드롭 검증 끝난 후**에 진행
  - Week 1 우선순위 = 음원/오디오 통합 후순위, JSON → 노트 line/length/timing 시각 검증 우선
- **Outcome**:
  - `JUDGMENT_SPEC.md §4` 미확정 6건 ✅ 해소 (Plain Mode 외 클래스/시너지만 잔존)
  - `V2_ARCHITECTURE_SPRINT1.md §4.1` UI placeholder 결정 신설 + Week 1 우선순위 노트
  - `CURRENT_STATUS.md` 블로커 = 없음 / 펜딩 = 곡 메타·시온 username·더미 채보·카드 데이터셋만
  - 한울에게 디코로 갈 사항 = 결정 사항 통보 + 카드 데이터셋 진행 체크 (질의는 0건)

---

## 2026-05-17: Sprint 1 진입 전 spec 9개 + URP → Built-in 다운그레이드

- **Context**: 5-agent council (Steelman/Red Team/Context Keeper/Pragmatist) 2 round 심의 결과 — 9개 진입 전 spec 박을 항목 + Sprint 1 = load-bearing spike 선언. v2 manifest가 URP 17.2.0인데 v1은 Built-in이라 prefab salvage 시 핑크 박스 risk.
- **Decision**:
  - URP 패키지 제거 → Built-in 유지. Hi-Fi 디자이너 합류 후 URP 재검토
  - Bar 위치 공식: 중심정렬 `Bar_N x = N - 12.5` (Bar_1=-11.5 ~ Bar_24=+11.5)
  - LANE_WIDTH = 1.0 unit (24 × 1.0 = 24 unit 폭, ortho size 10 + 16:9 화면 ~35.6 unit 안에 fit)
  - NoteScreen.cs 역할 = 24 Bar 컨테이너 + Bar 활성 토글 관리
  - Bar 콜라이더 9.6 unit y = lane 입력 식별 only, 판정은 거리 기반 (코드 주석 필수)
  - Conductor 초기화 = PlayScheduled(dspTime + 0.1) + Pause offset 누적 (Scenario A 차단)
  - PhysicsRaycaster를 Main Camera에 부착 (InputSystemUIInputModule은 Canvas만)
  - Note 책임 = prefab 4종 + Note.cs logic only + HoldNoteBody 별개 + FlickNote prefab variant 4개
  - 씬 루트 8 → 6 축소 (HUDController + NoteCreator → GameLoop 흡수)
  - Sprint 1 = "load-bearing spike" 명시
- **Outcome**: V2_ARCHITECTURE_SPRINT1.md 갱신, manifest.json URP 제거, plan 작성 (docs/superpowers/plans/2026-05-17-sprint1-plain-mode-playscreen.md)

---

## 2026-05-19: Sprint 1.5 — Card Effect Framework 설계 결정 기록

- **Context**: Sprint 1.5에서 카드 효과 프레임워크 (ICardEffect, CardSystem, CardEffectRegistry, 더미 3종) 신설. 여러 설계 지점에서 결정이 필요했음.
- **Decision**:
  1. **ICardEffect 5-method contract**: `OnAttach` / `OnDetach` / `GetCurrentModifiers` / `TryConsumeComboProtection` + `Id` 프로퍼티. "카드가 직접 이벤트 구독"하는 패턴 — CardSystem이 이벤트를 중개하지 않음. 효과마다 독립적 구독/해제로 생명주기 관리.
  2. **RuntimeInitializeOnLoadMethod 등록**: DummyEffectsBootstrap이 `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]`로 PlayMode 진입 시 자동 등록. EditMode 테스트에서는 미실행 → `CardEffectRegistry.Register()`를 테스트 코드에서 명시 호출.
  3. **Dictionary 삽입 순서 가정**: `CardEffectRegistry` 내부 `Dictionary<string, Func<ICardEffect>>`는 .NET Mono (Unity) 환경에서 삽입 순서 보존이 경험적으로 확인됨 (Sprint 1.5 3 entries). Sprint 2에서 카드 수 증가 시 리사이즈로 순서 깨질 리스크 있음 → **Sprint 2에서 `List<(string, Func<ICardEffect>)>`로 교체 고려**.
  4. **콤보 보호 패턴**: DummyDef_HeartQ는 Miss 이벤트 수신 시 `_pendingProtect=true` 세팅 → GameLoop가 `TryConsumeComboProtection()` 호출 시 소비. 이벤트 발행과 소비를 분리하는 2-step 패턴.
  5. **score 계산 순서**: `Publish(NoteJudgedEvent)` → 카드들이 내부 상태 업데이트 → `GetScoreMultipliers()` 호출 → product 곱셈 → `RoundToInt`. 이벤트 발행이 modifier 수집보다 반드시 선행해야 함.
- **Outcome**: 통합 검증 2026-05-19 PASS — ATK-only 1,320,000, ATK+SUP 1,584,000, 36/36 EditMode tests, Gate A(메모리 누수 없음) + Gate B(삽입 순서 보존) 모두 PASS.

## YYYY-MM-DD: [다음 결정 제목]

- **Context**:
- **Decision**:
- **Outcome**:
