# Rhyffle — 결정 기록

> rhyffle 지식 정본. WSL work-context가 아니라 이 파일이 source of truth.
> 형식: `## YYYY-MM-DD: 제목` / Context / Decision / Outcome

---

## 2026-05-23: Notion mirror 폐기 + 로컬 markdown snapshot 정본화 + MCP HTTP/OAuth 전환

- **Context**:
  - 공식 Notion 워크스페이스가 한울 개인 소유 → 사용자가 멤버여도 personal connection 부여 권한 없음
  - 시도 1: 사용자 개인 Notion에 공식 컨텐츠 복붙 → **paste 시 자동으로 휴지통 직행** (특히 데이터베이스). Database paste는 Notion 자체 제약으로 stub만 archived 처리됨
  - 시도 2: `Ctrl+Shift+V` (plain text paste) → trash는 회피했지만 페이지 mention이 **링크 블록**으로만 변환되어 본문 복제 안 됨 → MCP fetch 시 공식 워크스페이스 ID(`1e436a63...`) 404
  - 휴지통 비워도 paste-trash 재발 (mention/link reference가 stub 데이터소스 자동 생성 후 archived 트리거 가설)
- **Decision**:
  - **mirror 방식 폐기**. 사용자가 Notion에서 **Export Markdown & CSV → `.claude/context/notion_snapshots/`** 에 풀어서 두는 방식으로 전환. markdown이 spec 정본
  - 공식 페이지 변경 시 사용자가 export 갈아끼움. Claude는 로컬 파일 직접 read
  - Notion MCP 자체는 유지하되 **HTTP/OAuth 방식 (`https://mcp.notion.com/mcp`)** 로 전환 (기존 internal integration 토큰 방식 → OAuth). 사용자 개인 워크스페이스 접근 용도로만 사용
  - `.gitignore`: `notion_snapshots/**/*.zip` + 이미지/영상 확장자들 제외 (markdown만 git 추적)
- **Outcome**:
  - `.mcp.json` 의 notion 항목 제거, `~/.claude.json` project config에 HTTP transport 등록
  - 첫 export로 Rhyffle HQ 전체 (기획 / 회의록 / Roadmap / API 명세 / UI UX / 채보 디자인 툴 / 족보 시스템 밸런싱 / Albums / Game Sound / Members / Recruit / Font / Unity / 협업 방식 / QA / Brainstorming) 확보. 약 30개 markdown 파일
  - SPEC.md 인덱스 전면 갱신 (Notion URL → 로컬 경로)

---

## 2026-05-23: v2 정본 spec — 1차(포커)와 2차(COSMO) 보완 관계 파악

- **Context**:
  - Notion 로컬 export에서 `게임 시스템` 페이지 2개 + `카드 시스템` 페이지 2개 발견
  - ID prefix로 시점 식별: `26736a63...` = 2025-09-07 (v2 1차), `35d36a63...` = 2026-05-11 (v2 2차)
  - 두 페이지가 **단순 중복이 아닌 보완 관계**:
    - v2 1차 (포커 기반): 7 라인 / 56장 덱 / 4문양 13랭크 / 족보 12종 / 카드군 12종 / 양면 카드 + 능력 (고유+일반) / 반물질
    - v2 2차 (COSMO 통합): 8 라인 / 40장 덱 / COSMO Objekt 데이터 모델 / JSON 스키마 / 클래스+시너지 점수 / 강화 (화음/공명 각 10단계)
  - v2 2차로 명확히 대체된 영역: 라인 수, 덱 크기, 카드 데이터 모델, 점수 계산 식
  - v2 2차에 없는 영역 (1차 spec이 살아있는 것으로 추정): 족보 12종, 카드군 12종, 양면 카드, 능력, 반물질 재화
  - 동시에 `족보 시스템 밸런싱` 페이지 활발히 작업 중 (5월 23일까지 갱신) → 족보는 데모/프로덕션 둘 다에서 정본일 가능성 높음
- **Decision**:
  - **두 페이지 모두 정본으로 등록** — SPEC.md 의 spec 표에 보완 관계 명시
  - 영역별로 v2 1차 / v2 2차 어느 게 살아있는지 표 형태로 정리 (라인/덱/카드/점수/강화/족보/카드군/양면/반물질)
  - **불확실 항목**: 족보/카드군/양면/능력/반물질이 데모에 포함되는지 — 한울 확인 필요. **Sprint 2 spec 도착 시점에 확인**
  - 가설: 데모(2차) = COSMO 시너지만, 풀 프로덕션(1차+2차) = 포커 매커닉 + 카드군까지
- **Outcome**:
  - SPEC.md "버전 경계" 섹션 재작성 (표 형태로 영역별 정본 명시 + 불확실 항목 별도 마킹)
  - GLOSSARY.md 신설/대폭 확장 (포커 매커닉 + COSMO Objekt 필드 + 강화 + 족보 12종 배수 + 채보 단위 + 그룹 멤버 등)

---

## 2026-05-23: 5/21 디코 카드 DB 작업에서 발생한 spec 변경 미반영 사항

- **Context**:
  - 5/21 디코 thread `1506653488636235807` 에서 한울+동욱(`giantdaegari`) DB 입력 작업 진행
  - 다수 spec 결정이 디코에서만 합의되고 노션 페이지에는 미반영
- **Decision**: 다음 항목들을 spec 갱신 펜딩으로 트래킹
  - **Class 명칭 Premier → Special** (tripleS/ARTMS class enum 변경). 카드 시스템 페이지에는 옛 표기 (`First → Double → Special → Premier`) 그대로
  - **아덴(ARDEN) 유닛 1차 데이터셋 제외** (Sprint 2 범위 결정)
  - **카드 이미지 파일명 규약 확정**: `<그룹>_<시즌>_<멤버>_<시리얼>` (예: `ARTMS_Divine01_Heejin_120Z.jpg`)
  - **이미지 경로 = 클라가 prefix** (DB는 파일명만 보유)
  - **그룹별 포맷 분기 룰**: tripleS/ARTMS 동일 enum, idntt 별도 enum
  - **멤버 규모**: 현재 44명, 9명 추가 예정 = 53명
  - **DB 오타 = 한울 일괄 검수** → 클라 검증 strict 가능
  - **giantdaegari ↔ 이동욱 매핑 확정** (GitHub repo `gari0525/Rhyffle-jokbo` 100만회 시뮬레이션 코드가 결정적 증거)
- **Outcome**:
  - TEAM.md 갱신 (이동욱 = `gari0525` 확정)
  - DISCORD_LOG.md 5/21 항목에 spec 갱신 펜딩 명시
  - 한울에게 확인 필요: Class enum 노션 페이지 반영 / 데모 spec 범위 (포커 매커닉 포함 여부)

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

---

## 2026-05-19: Sprint 1.5 — Card Effect Framework

- **Context**: Sprint 1.5에서 카드 효과 발동을 score/combo 로직과 분리하는 이벤트 기반 프레임워크 필요.
- **Decision**: EventBus + ICardEffect interface + CardSystem (Awake에서 registry pull). 카드가 직접 이벤트 구독/해제 — CardSystem이 중개하지 않음. How: `Cards/CardSystem.cs` + `Cards/Effects/*.cs`.
- **Outcome**: DummyEffect 3종 (ATK, SUP, DEF) + runtime 검증 PASS. 36/36 EditMode tests.

---

## 2026-05-19: Sprint 1.5.1 — Lane Visualization + Card-Lane Mapping

- **Context**: 사용자가 점 vs 띠 시각화 명시 요구 + 카드 lane 매핑 spec 확정 필요.
- **Decision**: Bar 24→25 점 marker (입력 없음), LaneAnchor 24 입력 collider, CARDS_IN_FIELD 7→8, LANES_PER_CARD=3, lane→card 영역 한정 modifier. `LaneToCardIndex` 헬퍼, `CardSystem.GetScoreMultipliersForSlot`.
- **Outcome**: Bar/LaneAnchor 역할 분리 완료. 40/40 tests PASS.

---

## 2026-05-19: Sprint 1.5.1 — CardBoard World Space Refactor

- **Context**: ScreenSpace-Camera Canvas 자식으로 카드 보드 배치 시 화면 비율 의존 발생.
- **Decision**: CardBoard를 WorldSpace Canvas (`CardBoardCanvas`, position y=-5, size 24×4.5) 신설 아래로 이전. 슬롯 width=3 world unit.
- **Outcome**: 화면 비율 의존 제거, lane area 자동 정렬. 40/40 tests PASS.

---

## 2026-05-19: Sprint 1.5.2 — PokerHandEvaluator 12종

- **Context**: 기존 7 전통 hand만으로 spec 불완전. Rhyffle 고유 4종 추가 필요.
- **Decision**: 7 전통 + 4 고유 (럭키세븐=7 연속, 더블트리플, 레인보우, 쓰리페어) = 12종. `PokerHandEvaluator.Evaluate` priority chain.
- **Outcome**: 22 EditMode tests 포함 58/58 PASS.

---

## 2026-05-19: Sprint 1.5.3 — Figma Lo-Fi 배치 Placeholder

- **Context**: Figma Lo-Fi 배치에 맞춰 HUD 재배치 + placeholder 5종 + 영역 시각화 필요.
- **Decision**: Score/Combo 우상단 재배치, 일시정지/곡정보/셔플/덱묘지 placeholder 4종, 노트/판정선/카드 영역 visual band 3종 (World Space SpriteRenderer — Canvas overlay 좌표 화면 비율 의존 문제 해소). `CreatePlaceholderPanel` + `CreateWorldVisualBand` 헬퍼.
- **Outcome**: Lo-Fi 시각 align 완료. 58/58 tests PASS.

---

## 2026-05-19: Sprint 1.5.4 — TMP 한글 폰트

- **Context**: 한글 라벨 표시 필요. MalgunGothic은 license 불명확 (MS Windows 번들).
- **Decision**: NotoSansKR SDF (Apache 2.0, license-clean)로 교체. Dynamic SDF atlas Texture2D + Material을 `AssetDatabase.AddObjectToAsset`으로 sub-asset 저장 (MissingReferenceException 방지). LiberationSans fallback chain 갱신.
- **Outcome**: 한글 라벨 표시 + license 안전. MalgunGothic 파일 삭제. 58/58 PASS.

---

## 2026-05-19: Sprint 1.5.5 — 일시정지 버튼 Wire

- **Context**: PausePlaceholder가 시각 요소만 있었고 실제 Pause/Resume 기능 없음.
- **Decision**: `PauseButton` MonoBehaviour (PausePlaceholder Image + Button + click handler). `GameLoop.IsPaused` getter 추가. 클릭 시 라벨 PAUSE↔PLAY 토글.
- **Outcome**: Pause/Resume 기능 동작 확인. 58/58 tests PASS.

## YYYY-MM-DD: [다음 결정 제목]

- **Context**:
- **Decision**:
- **Outcome**:
