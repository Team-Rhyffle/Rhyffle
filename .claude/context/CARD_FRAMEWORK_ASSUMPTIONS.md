# Card Effect Framework — Sprint 1.5 Assumptions & Stubs

> Sprint 1.5 에서 의도적으로 stub 처리한 영역 목록.
> 각 항목은 "지금 뭘 하드코딩/스텁했는지" + "어디를 나중에 수정해야 하는지" 를 명시.
> 실제 spec이 확정되면 아래 "재방문 파일" 부터 열어볼 것.

---

## 1. 묘지 / 덱

| 항목 | 내용 |
|------|------|
| **현재 stub** | 카드 풀 하드코딩 (DummyAtk_SpadeK / DummyDef_HeartQ / DummySup_DiamondJ 3종). 게임 시작 시 풀에서 7장 랜덤 픽. 묘지 없음 — 사용한 카드는 단순 비활성화. |
| **재방문 파일** | `CardEffectRegistry.cs` (풀 정의), `CardSystem.cs` (드로우/덱 관리 로직) |
| **트리거** | 김한울 card dataset (Sprint 2 입수 예정) — ~400장 데이터 도착 후 풀/덱/묘지 로직 전면 교체 |

---

## 2. 키 노트 정의

| 항목 | 내용 |
|------|------|
| **현재 stub** | "키 노트" spec 미정. 매 32 콤보마다 `KeyNoteProcessedEvent` 발행하는 것으로 대체. 간격 값 = `GameConfig.KEY_NOTE_COMBO_INTERVAL = 32`. |
| **재방문 파일** | `GameLoop.cs` (ApplyJudgment 내부 — 이벤트 발행 로직), `GameConfig.cs` (`KEY_NOTE_COMBO_INTERVAL` 상수 값 교체 또는 삭제, 채보 키 노트 조건으로 대체), channel spec extension (이벤트 페이로드) |
| **트리거** | 채보/플레이 채널 spec 에서 "키 노트" 조건 확정 시 |

---

## 3. 일반능력 random 발급

| 항목 | 내용 |
|------|------|
| **현재 stub** | Sprint 1.5 에서 완전 스킵. 더미 카드 3종은 모두 고유 능력(unique ability)만 보유 — 일반능력 없음. |
| **재방문 파일** | `CardEffectRegistry.cs` (등록 로직 내 일반능력 생성 경로 — rarity 기반 랜덤 발급 제너레이터 추가 예정) |
| **트리거** | 카드 rarity / 일반능력 목록 spec 확정 시 |

---

## 4. 카드 UI 위치

| 항목 | 내용 |
|------|------|
| **결정 (2026-05-19 사용자 검수)** | 카드 1개 = lane 3개 폭. 8 카드 슬롯 (CARDS_IN_FIELD=8, LANES_PER_CARD=3). 슬롯 width = Canvas RefWidth / 8 ≈ 119.5 px. 좌→우 horizontal 배열, 화면 하단 anchor. |
| **Hi-Fi 적용 방침** | Hi-Fi Figma 적용 시 슬롯 외관(이미지/폰트/색)만 교체. 슬롯 수/폭/위치 layout은 spec 확정. |
| **재방문 파일** | `CardBoardUI.cs` (Hi-Fi 외관 적용), `Game.unity` (Canvas 자식 외관 변경) |
| **트리거** | Hi-Fi Figma 외관 확정 후 |

---

## 5. 카드 data source

| 항목 | 내용 |
|------|------|
| **현재 stub** | C# 코드 내 하드코딩 3종 (DummyAtk_SpadeK / DummyDef_HeartQ / DummySup_DiamondJ). JSON loader 없음. |
| **재방문 파일** | 신규 `CardDataLoader.cs` (JSON 파싱), `CardEffectRegistry.cs` (등록 교체), `StreamingAssets/cards.json` (데이터 파일) |
| **트리거** | 김한울 card dataset (~400장) Sprint 2 입수 후 JSON 포맷 합의 → loader 구현 |

---

## 6. lane → card 영역 매핑

| 항목 | 내용 |
|------|------|
| **현재 stub/하드코드** | `GameConfig.LaneToCardIndex(lane) = lane / 3` (lane 0~2 → card 0, lane 3~5 → card 1, ..., lane 21~23 → card 7). 카드 modifier 적용 시 영역 한정. 매핑은 단순 정수 나눗셈으로 결정 (left→right 슬롯 배치 가정). |
| **재방문 파일** | `GameConfig.cs` (LANES_PER_CARD 변경 시), `CardSystem.cs` (`GetScoreMultipliersForSlot` 추가는 T5), `GameLoop.cs` (ApplyJudgment 영역 필터 — T5) |
| **트리거** | 카드 배치 방식 spec 변경 시 (예: 카드 2개 폭 사용, 비대칭 매핑 등) |

---

*최종 갱신: 2026-05-19 (Sprint 1.5.1 T3)*

### 변경 이력
- `2026-05-19`: Sprint 1.5 Task 0 — 최초 작성 (항목 1~5 stub).
- `2026-05-19`: Sprint 1.5.1 T3 — 카드 UI 위치 stub → 결정 전환 (항목 4), lane→card 매핑 항목 신규 (항목 6).
