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
| **현재 stub** | Lo-Fi Figma 미참조. 화면 하단에 7 슬롯 수평 나열, 각 슬롯 ~80×120 px placeholder 박스. 위치/간격은 임의. |
| **재방문 파일** | `CardBoardUI.cs` (슬롯 배치 로직), `Game.unity` Canvas 하위 CardBoard GameObject |
| **트리거** | Figma Lo-Fi 레이아웃 확정 후 (Hi-Fi 합류 전 단계 갱신 OK) |

---

## 5. 카드 data source

| 항목 | 내용 |
|------|------|
| **현재 stub** | C# 코드 내 하드코딩 3종 (DummyAtk_SpadeK / DummyDef_HeartQ / DummySup_DiamondJ). JSON loader 없음. |
| **재방문 파일** | 신규 `CardDataLoader.cs` (JSON 파싱), `CardEffectRegistry.cs` (등록 교체), `StreamingAssets/cards.json` (데이터 파일) |
| **트리거** | 김한울 card dataset (~400장) Sprint 2 입수 후 JSON 포맷 합의 → loader 구현 |

---

*최종 갱신: 2026-05-19 (Sprint 1.5 Task 0)*
