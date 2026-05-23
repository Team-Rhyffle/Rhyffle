using System.Collections.Generic;

/// <summary>
/// 덱 데이터 공급원 추상화. DeckSystem이 이 인터페이스를 통해 카드 목록을 받아 초기화.
///
/// 구현체:
/// - DummyDeckSource: Sprint 1.5.6 더미 24장 (현재 default, 데이터셋 도착 전까지)
/// - JsonDeckSource (예정): 한울+동욱 카드 DB JSON 로드 (Sprint 2 입력 자산)
/// </summary>
public interface IDeckSource {
    List<CardData> LoadDeck();
}
