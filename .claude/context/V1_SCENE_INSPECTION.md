# Rhyffle v1 — Game.unity 씬 인스펙션 (Unity MCP 추출)

> 2026-05-17 Unity MCP for Unity로 v1 (`C:/Users/ljk91/OneDrive/문서/GitHub/Rhyffle`) 실시간 인스펙션 결과.
> 코드 reading만으로 안 보이는 인스펙터 값 / GameObject 와이어링 / 컴포넌트 활성화 여부를 정리한 reference.
> v2 Sprint 1 셋업 시 placeholder 갱신 근거.

---

## 1. 프로젝트 메타

| 항목 | 값 |
|---|---|
| Unity 버전 | 6000.2.8f1 (v2와 동일 ✓) |
| Build platform | Android |
| Editor platform | Windows |
| 활성 씬 | `Assets/Scenes/Game.unity` (= 플레이 씬 자체) |
| Tags (커스텀) | `Bar`, `HoldNote`, `card`, `Deck`, `Slot` |
| Layers | Default, TransparentFX, Ignore Raycast, Water, **UI(5)** — 커스텀 없음 |
| Input | **Unity Input System** (legacy Input X — EventSystem이 `InputSystemUIInputModule` 사용) |

---

## 2. Game.unity 씬 hierarchy (루트 10개)

```
Main Camera                  (Camera + AudioListener)
Canvas                       (RectTransform + Canvas + Scaler + Raycaster) — UI 컨테이너
EventSystem                  (InputSystemUIInputModule)
GameSystem
  ├─ NoteScreen              (pos 0,-2.5,0; NoteScreen 컴포넌트)
  │   ├─ Bar_1 ~ Bar_21      (21개 lane anchor)
  │   ├─ FlickUpZone         (빈 Transform — flick 영역 마커)
  │   └─ FlickDownZone       (빈 Transform — flick 영역 마커)
  ├─ CardBoard               (pos 0,10,0; scale 43x18; RectTransform + SpriteRenderer)
  └─ NoteBoard               (pos 0,0,0; LineRenderer — hold note 폴리라인용)
GameUIManager                (Pause 카운트다운 컨트롤러)
GamePlayer                   (메인 게임 로직, 569줄)
NoteTester                   (비활성. NoteTest 컴포넌트)
NoteCreator                  (note prefab 5종 ref)
StandardCard                 (씬에 박힌 카드 1장 — 데모용)
Audio Source                 (pos 16.12,3.52,0; AudioSource + AudioTest)
```

---

## 3. Camera (Main Camera, instanceID 39752)

| 속성 | 값 |
|---|---|
| Transform | pos (0, 1.5, -10), rot 0, scale 1 |
| **Projection** | **Orthographic** (ortho=true, size=10) |
| Near / Far | 0.3 / 1000 |
| Background color | RGB (49, 77, 121) / alpha 0 |
| Clear flags | 1 = Solid Color |
| Depth | -1 |
| AudioListener | Camera에 부착 (Audio Source는 별도 GameObject) |

→ v2 권장: 동일하게 ortho size 10, pos (0, 1.5, -10) 그대로 재현. AudioListener도 Camera에 부착.

---

## 4. Bar 레이아웃 (21 lane)

샘플 좌표 (모두 parent = NoteScreen, NoteScreen pos = (0, -2.5, 0)):

| GameObject | World position | localScale |
|---|---|---|
| Bar_1 | (-9, -2.5, 0) | (3.2, 3.2, 1) |
| Bar_4 | (-6, -2.5, 0) | (3.2, 3.2, 1) |
| Bar_8 | (-2, -2.5, 0) | (3.2, 3.2, 1) |

→ 공식: **Bar_N world x = -10 + N** (Bar_1=-9, Bar_11=+1, Bar_21=+11)
→ **레인 간격 = 1.0 unit**
→ **판정선 y = -2.5** (모든 Bar의 world y. v2 spec의 placeholder -2.25는 부정확 — v1 실측은 -2.5)
→ Bar 가로 전체 span = 20 unit (Bar_1 ~ Bar_21)
→ 카메라 ortho size 10 → 화면 vertical 20 unit, horizontal = 20 × aspect

### 4.1 Bar_8 컴포넌트 (대표 인스펙터)

- **SpriteRenderer (enabled=false ⚠️)**: 런타임에 보이지 않음. 단지 anchor/collider 컨테이너
  - sprite: `Assets/Resources/Art/barTest.png`
  - color white, sortingOrder 10
  - localBounds.size = (0.32, 0.16, 0.2) — sprite 원본 크기
- **BoxCollider (3D)**: center (0, -0.84, 0), size (0.32, 3.0, 0.2)
  - localScale 3.2 적용 → 월드 콜라이더 size = (1.024, 9.6, 0.64)
  - 월드 y 범위 = -5.188 ± 4.8 = (-9.988, -0.388) — bar 위치보다 아래로 길게 뻗음 (플레이 영역 터치 영역)
- **Bar 컴포넌트**: `barNum` (1~21), `gamePlayer` ref (인스펙터 와이어링)

→ v2 시사점:
- bar 시각화 = sprite 비활성 + 그냥 collider anchor로 사용. v2도 placeholder 단계는 같은 방식 가능
- **3D BoxCollider** + 3D Physics 사용 (BoxCollider2D 아님). ortho camera + 3D physics 조합

---

## 5. UI (Canvas, instanceID 40112)

### 5.1 Canvas 자체

| 속성 | 값 |
|---|---|
| renderMode | **1 = ScreenSpaceCamera** (worldCamera = Main Camera, planeDistance 100) |
| Position | (0, 1.5, 90) world (90 = planeDistance - camera.z = 100 - 10) |
| Scale | 0.0583 (Camera-space에서 자동 산출됨) |
| CanvasScaler | **ScaleWithScreenSize**, ref res **956 × 440**, ScreenMatchMode = Expand (2), match 0 |
| referencePixelsPerUnit | 100 |
| Layer | UI (5) |

→ v2 시사점: 956×440 = **landscape 21.7:10 비율** (모바일 landscape 기준). Sprint 1 placeholder 동일 채택 권장.

### 5.2 Canvas 자식 8개 (식별된 것만)

| 자식 GameObject | Anchor | anchoredPosition | sizeDelta | 용도 |
|---|---|---|---|---|
| **Score** (40104) | (0, 0.5)–(0, 0.5) — 좌측중앙 | (25, 0) | (100, 0) | 점수 텍스트 |
| **JudgementText** (40024) | (0,0)–(1,1) — stretch | (0, -50) | (-400, -400) | 화면 중앙 판정 텍스트. **fontSize 60, bold, center align** |
| **Combo** (39888) | (1, 0.5)–(1, 0.5) — 우측중앙 | (-36, 0) | (80, 30) | 콤보 부모 컨테이너 (자식에 실제 텍스트) |
| ↳ Combo (40206) | (0.5, 1)–(0.5, 1) — 상단중앙 | (0, 0) | (50, 0) | 콤보 텍스트 (수치) |
| 나머지 5개 (39850, 39992, 40430, 40424, 40266) | 미조사 | — | — | 추가 UI 요소 (필요 시 드릴) |

⚠️ **v1 vs v2 spec 차이**:
- v1 Score = **좌측중앙**
- v1 Combo = **우측중앙** (parent), 내부 텍스트는 상단중앙
- v2 사용자 결정 (2026-05-17) = **콤보 중앙상단** — v1과 다름. v2는 의식적으로 위치 바꾸는 거

### 5.3 Font

- TMP `LiberationSans SDF` (Unity default) 사용 — JudgementText / TempJudgeText 동일
- v2도 default TMP font 그대로 시작 가능 (placeholder 단계)

---

## 6. AudioSource (Audio Source, instanceID 40184)

| 속성 | 값 |
|---|---|
| clip | `Assets/Resources/Sounds/Songs/06_daki_우산_아래_(slow_the_rain).wav` (tripleS Daki) |
| volume | 0.3 |
| pitch | 1.0 |
| spatialBlend | 0 (= **2D 사운드**) |
| playOnAwake | **false** (코드에서 명시적으로 Play() 호출) |
| loop | false |
| AudioTest 컴포넌트 | `timer 0`, `paused false` (자체 시간 추적) |

→ v2 시사점:
- v1 시간 소스는 **AudioSource.time** (AudioTest.timer가 보조). v2는 council 결정대로 **dspTime** 으로 갈 것 (jitter 회피)
- 음원 자산 경로 패턴 = `Resources/Sounds/Songs/{nn}_{artist}_{title}.wav`
- 2D 음원 (spatial 0) — v2 동일

---

## 7. GamePlayer (GameObject 39840)

### 7.1 GamePlayer 컴포넌트 인스펙터

- **입력 상태 배열들 = 모두 length 21** (lane 21 확정):
  - `press`, `slide`, `intouch`, `endtouch`, `flickUp`, `flickDown` — bool[21]
  - `touchStart`, `touchEnd`, `touchCon` — int[21] (시간 stamp)
- Refs (모두 인스펙터 와이어링):
  - `scoreInfo` → GameScoreInfo
  - `judgeText` → TempJudgeText TMP
  - `audio` → AudioTest 컴포넌트
- `play=true`, `currentTime=-1000.0`, `hitEvents=[]` (런타임 초기값)

### 7.2 TouchVisualizer

- `touchCircle` prefab ref: `Assets/Resources/Prefabs/UI/touchCircle.prefab`

### 7.3 GameScoreInfo

- `text` → Score TMP, `judgementText` → JudgementText TMP
- `handRankScore[13]`, `scaleAdd[7]`, `scaleMulti[7]` — **모두 0 초기값** (Plain Mode만 사용, Challenge Mode 영역)
- `baseHandRank=0`, `allRankBonus=0`, `firstNote=true`

→ v2 시사점:
- 인스펙터 와이어링 방식 (코드에 GetComponent 캐시 없음) — v2 GameLoop은 동일하게 [SerializeField] 노출 또는 council 결정대로 명시적 ref 패턴
- Score/Combo/Judgement 분리는 v1 이미 잘 됨 (score 분리 = ScoreSystem salvage 1h 추정 근거)

---

## 8. GameUIManager (GameObject 39862)

- `gamePlayer` ref → GamePlayer
- **`countSprites[3]`** = `Assets/Resources/Art/UI/Pause/1.png`, `2.png`, `3.png`
- `audioTest` ref → AudioTest 컴포넌트

→ v2 시사점: **Pause/Resume 카운트다운 = 3-2-1 sprite 시퀀스**. 사용자 결정 "Pause UX v1 참고" 의 구체 의미 = 이 sprite 3장 salvage + 동일 표시 로직.

---

## 9. NoteCreator (GameObject 40122) — 노트 프리팹 ref

5개 ref 모두 `Assets/Resources/Prefabs/Note/`:

| 필드 | 경로 |
|---|---|
| `basicNote` | `Resources/Prefabs/Note/BasicNote.prefab` |
| `slideNote` | `Resources/Prefabs/Note/SlideNote.prefab` |
| `flickNote` | `Resources/Prefabs/Note/FlickNote.prefab` |
| `holdNote` | `Resources/Prefabs/Note/HoldNote.prefab` |
| `holdNoteBody` | `Resources/Prefabs/Note/HoldNoteBody.prefab` |

→ v2 시사점: 노트 프리팹 salvage 시 5개 그대로 (HoldNote + HoldNoteBody 둘 다 필요). 위 경로 그대로 v2 `Resources/Prefabs/Note/` 로 복사.

---

## 10. NoteScreen (GameObject 40442) + NoteBoard (40098)

- `NoteScreen` 컴포넌트: 21개 Bar + FlickUpZone + FlickDownZone 부모. NoteScreen이라는 별도 MonoBehaviour 존재 (코드 reading 필요)
- `NoteBoard` 컴포넌트: `LineRenderer` 부착. **hold note 폴리라인 공유 리소스** (각 hold마다 별개 LineRenderer 만들지 않고 공유 가능성)
- `CardBoard` (39958): 카드 디스플레이 영역. 4 자식 (StandardCard 등). Sprint 1 제외 영역

---

## 11. NoteTester (40480, 비활성)

- 활성 상태 false (씬에 disable로 박혀있음). `NoteTest` 컴포넌트.
- v1 개발 시 사용한 노트 동작 검증 도구. v2 Sprint 1 동일 패턴 권장 — 더미 채보 없이 코드 트리거로 노트 1개 떨어뜨리기.

---

## 12. v2 spec placeholder 갱신 권고 사항

| spec 항목 | 기존 placeholder | v1 실측 | 권고 |
|---|---|---|---|
| `JUDGE_LINE_Y` | -2.25 (v1 "참고치") | **-2.5** (Bar y) | **갱신** → -2.5 |
| 라인 간격 (`LANE_WIDTH`) | "화면 fit 임의" | **1.0 unit** (Bar_N+1 - Bar_N) | **갱신** → 1.0 (또는 24 lane fit 위해 21/24 = 0.875 비례 축소) |
| 카메라 ortho size | 미정 | **10.0** | spec 등록 |
| 카메라 position | 미정 | **(0, 1.5, -10)** | spec 등록 |
| Canvas 모드 | 미정 | **ScreenSpaceCamera, ref 956×440** | spec 등록 |
| 콤보 UI 위치 | 사용자 결정 "중앙상단" | v1: 우측중앙 | v2 의식적 변경 — 그대로 유지 |
| 점수 UI 위치 | 사용자 결정 "v1 참고" | v1: 좌측중앙 anchor (25,0) | v2 spec 명시화 |
| 판정 UI | 사용자 결정 "v1 참고" | 화면 중앙, fontSize 60 bold, anchor stretch + 400px margin | v2 spec 명시화 |
| Pause UI | 사용자 결정 "v1 참고" | 3-2-1 sprite (`Resources/Art/UI/Pause/{1,2,3}.png`) | v2 salvage 대상 명시 |
| 노트 프리팹 | salvage 예정 | 5종 in `Resources/Prefabs/Note/` | 경로 그대로 v2 복사 |
| Bar 시각화 | 미정 | SpriteRenderer enabled=false (placeholder) | v2도 동일 — placeholder 단계 |

---

## 13. 추가 인스펙션 대상 (필요 시 드릴)

아직 안 본 것들:
- BasicNote.prefab 인스펙터 (sprite scale.y로 `h` 실측치 확인) — 가장 우선순위 높음
- SlideNote/FlickNote/HoldNote/HoldNoteBody 인스펙터
- NoteScreen.cs / NoteBoard.cs 컴포넌트 코드 (LineRenderer 사용 패턴)
- Canvas 나머지 5개 자식 (39850, 39992, 40430, 40424, 40266)
- ProjectSettings (Time fixedDeltaTime, Player orientation, Quality vSync)
- Resources/Sounds/Songs 폴더 전체 곡 목록
- v1에 깔린 패키지 목록 (manifest.json) — v2와 호환성

---

## 14. 변경 이력

- 2026-05-17: Unity MCP for Unity로 v1 인스턴스 (8080) 연결 + Game.unity 인스펙션. 본 문서 신설.
