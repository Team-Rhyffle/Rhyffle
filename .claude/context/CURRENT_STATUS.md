# Rhyffle v2 — Current Status

> **작업 포인터**. 새 세션 시작 시 가장 먼저 읽을 파일.
> Phase 전환 / step 완료 / 결정 / 블로커 발생·해소 / 일시 중지 시점에 갱신.
> 자세한 결정 근거는 `DECISIONS.md`, 작업 계획은 `V2_ARCHITECTURE_SPRINT1.md`, spec 인덱스는 `SPEC.md`.

---

## 현재 단계
**Sprint 1.5.6 완료** — DeckSystem + Graveyard. **62/62 EditMode tests PASS** (5/19).  
**Sprint 2 prep T1+T2+T3 완료** — 83/83 PASS.  
**Sprint 2 진입 대기** — 카드 데이터셋 입력 진행 + Sprint 2 수치/UX spec 한울 작업 대기. **데모 범위 = v2 2차 (COSMO) only 확정 (5/24 한울)**.

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
- **JsonDeckSource 구현** — 단, JSON 라이브러리 결정 필요 (Unity JsonUtility 는 Dictionary 미지원). Newtonsoft 추가 vs 데이터 형식 list 변환 결정 필요 → 카드 DB 도착 후 진행 권장
- 5/22~5/23 다른 디코 채널 활동 확인

### 외부 입력 대기 (모두 한울 — Sprint 2 spec 본체)
- 카드 데이터셋 (≈400장 JSON) ETA — 한울 + 동욱
- 클래스 점수 수치 (Class별 +N)
- 시너지 점수 수치 (시너지_1 합연산 / 시너지_2 곱연산)
- 화음 / 공명 단계별 보너스 수치 (10단계)
- 셔플 키 활성화 콤보 게이지 임계값
- 키 노트 8장 교체 타이밍 / UX
- 덱 편집 UI/UX (동욱 작업)
- 카드 강화 UI/UX (동욱 작업)
- Class 명칭 Premier → Special 노션 페이지 반영 (한울)

## 블로커
- **Sprint 2 본격 진입 블로커**: 카드 데이터셋 + 위 수치/UX spec 도착 대기. 5/24 한울 spec 작업 진행 선언 (ETA 미확정)
- ~~**spec 불확실성**: v2 1차 매커닉 데모 포함 여부~~ → **해소 (5/24)**: 데모 = v2 2차 only 확정

## 펜딩 (사용자 / 외부)
- 카드 데이터셋 (≈400장) — 한울+동욱 입력 진행 중
- 클래스/시너지/화음·공명 수치 + 셔플/키노트 UX — 한울 spec 작업 중
- 덱 편집 / 카드 강화 UI/UX — 동욱 작업
- Class 명칭 Premier → Special 노션 반영 — 한울
- 곡 메타 spec (BPM/Offset/AudioPath 형식) — 이재근 작성. **후순위**
- 시온 (사운드) Discord username 매핑 — 사운드 작업 시작 시 보완
- 곡 정보 데이터 wire (chartName + audio 메타 spec — 펜딩)
- Figma Hi-Fi 외관 교체 (placeholder → 실제 art) — 디자이너 부재 동안 보류

### 폐기됨 (5/24 한울 확정으로 무효)
- ~~A-high straight (10-J-Q-K-A) spec~~ — 포커 매커닉 자체 미사용
- ~~등급 단계 4/5단계 정본~~ — 포커 rarity 미사용, Objekt class 체계로 대체
- ~~레인보우/럭키세븐 정의 spec drift~~ — 족보 시스템 미포함
- ~~족보 우선순위 충돌~~ — 족보 시스템 미포함
- ~~묘지/덱/키노트/일반능력 spec~~ 중 "일반능력" 부분 (랜덤 부여 능력) — v2 1차 매커닉이라 폐기. 묘지/덱/키노트는 펜딩 유지

## Sprint 2 입력 자산 — 신규 확정 사항 (5/21 디코)
- **아덴(ARDEN) 유닛 1차 데이터셋 제외** — 후속 추가 여지
- **Class 명칭: Premier → Special** (노션 페이지 미반영, 한울 액션 필요)
- **카드 이미지 파일명 규약**: `<그룹>_<시즌>_<멤버>_<시리얼>` (예: `ARTMS_Divine01_Heejin_120Z.jpg`)
- **이미지 경로는 클라가 prefix** (DB는 파일명만 보유)
- **그룹별 포맷 분기**: tripleS/ARTMS 동일 enum, idntt 별도 enum (Season 종류 / Class 명칭 차이)
- **멤버 규모**: 현재 44명 + 9명 추가 예정 = 53명
- **DB 오타는 한울 일괄 검수** → 클라 검증 strict 가능

## 카드 spec 정본 — 데모 = v2 2차 only 확정 (5/24)
| 영역 | 정본 |
|---|---|
| 라인 수 / 덱 크기 / 카드 데이터 모델 / 점수 식 / 강화 (화음/공명) | **v2 2차** (35d36a63) |
| 족보 12종 / 카드군 12종 / 양면 카드 / 고유능력+일반능력 / 반물질 재화 / 포커 4문양 13랭크 | **데모 미포함** (5/24 한울 확정). 향후 프로덕션 부활 가능성으로 코드/문서 보존 |

## 최근 변경
- 2026-05-24: **PokerHandEvaluator + 12종 PokerHand enum + 22 tests 삭제** (옵션 A 채택) — 데모 미포함 결정 적용. 외부 참조 0건 확인. Unity 테스트 카운트 83 → ≈65 예상 (Unity 실행 시 확정)
- 2026-05-24: **데모 범위 = v2 2차 only 확정** (한울 답변) — 족보/카드군/양면+반물질/포커 매커닉 데모 전면 폐기. 작업 분배: 한울=Sprint 2 spec 본체 / 동욱=덱 편집·카드 강화 UI/UX. 펜딩 항목 다수 무효화
- 2026-05-23: **Sprint 2 prep T3 완료** — CardMetadataRegistry (Dictionary lookup, GetByInstance) + 발견된 레인보우/럭키세븐 spec drift GLOSSARY align + 우선순위 코드↔spec 불일치 펜딩 트래킹. **83/83 PASS** (77 → 83)
- 2026-05-23: **Sprint 2 prep T2 완료** — CardInstance (개별 카드 DB JSON 매핑) + CardEnhancement (화음/공명 validator + applier, 10단계 cap, 12 tests). **77/77 PASS** (65 → 77)
- 2026-05-23: **Sprint 2 prep T1 완료** — CardMetadata 모델 + IDeckSource 추상화 + DummyDeckSource 추출 + DeckSystem refactor. **65/65 PASS** (62 → 65)
- 2026-05-23: **Notion 정본 markdown import + spec 분석** — mirror 폐기, v2 1차/2차 보완 관계 분석. SPEC.md / GLOSSARY.md / DECISIONS.md / TEAM.md 전면 갱신

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
