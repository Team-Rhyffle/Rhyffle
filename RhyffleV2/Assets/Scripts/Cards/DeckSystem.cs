using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sprint 1.5.6: 덱 + 묘지 시스템 (v1 DeckManager + CardManager 참고).
/// - 단일 Graveyard (v1의 UsedDeck/Cemetery 이중화 fix)
/// - 셔플 시 묘지 합쳐 재셔플 (TCG 표준)
/// - Draw trigger: KeyNoteProcessedEvent (Sprint 1.5 stub: 32 콤보 — 채보 spec 도착 시 교체)
///
/// Sprint 2 prep (2026-05-23): IDeckSource 추상화 추가. 기본은 DummyDeckSource.
/// 데이터셋 도착 시 JsonDeckSource 구현 후 SetSource(...) 로 교체.
/// </summary>
public class DeckSystem : MonoBehaviour {
    public const int DUMMY_DECK_SIZE = 24;

    public CardSystem cardSystem;

    private IDeckSource _source = new DummyDeckSource();

    private readonly List<CardData>  _deck      = new List<CardData>();   // 원본 (Awake 후 immutable)
    private readonly List<CardData>  _drawPile  = new List<CardData>();   // 셔플된 미사용
    private readonly Queue<CardData> _graveyard = new Queue<CardData>();

    private System.Action<KeyNoteProcessedEvent> _keyHandler;

    public int DeckCount      => _drawPile.Count;
    public int GraveyardCount => _graveyard.Count;

    void Awake() {
        InitDeck();
    }

    /// <summary>
    /// IDeckSource 교체. InitDeck() 호출 전에만 효과 있음. null 무시.
    /// 테스트 / Sprint 2 JsonDeckSource 주입용.
    /// </summary>
    public void SetSource(IDeckSource source) {
        if (source != null) _source = source;
    }

    /// <summary>
    /// EditMode 테스트용 초기화 진입점.
    /// EditMode NUnit에서는 Awake가 자동 호출되지 않으므로 SetUp에서 명시적으로 호출.
    /// 런타임에서는 Awake()가 호출함 — 중복 호출 무방 (idempotent).
    /// </summary>
    public void InitDeck() {
        _deck.Clear();
        _deck.AddRange(_source.LoadDeck());
        ResetDrawPileFromDeck();
    }

    void Start() {
        if (cardSystem == null) cardSystem = FindObjectOfType<CardSystem>();
        _keyHandler = OnKeyNote;
        EventBus.Subscribe(_keyHandler);

        // 게임 시작 시 초기 draw
        var initial = DrawN(GameConfig.CARDS_IN_FIELD);
        if (cardSystem != null) {
            cardSystem.SetField(initial);
            EventBus.Publish(new FieldChangedEvent { Field = cardSystem.GetField() });
        }
    }

    void OnDestroy() {
        if (_keyHandler != null) EventBus.Unsubscribe(_keyHandler);
        _keyHandler = null;
    }

    /// <summary>드로우 풀에서 n장 pop. 부족 시 ShuffleAll() trigger.</summary>
    public List<CardData> DrawN(int n) {
        var result = new List<CardData>();
        for (int i = 0; i < n; i++) {
            if (_drawPile.Count == 0) ShuffleAll();
            if (_drawPile.Count == 0) break;   // 묘지도 비어있는 극한 케이스
            var c = _drawPile[0];
            _drawPile.RemoveAt(0);
            result.Add(c);
        }
        return result;
    }

    /// <summary>키노트 → 필드 전체를 묘지로 + 8장 새로 draw.</summary>
    private void OnKeyNote(KeyNoteProcessedEvent evt) {
        if (cardSystem == null) return;
        var oldField = cardSystem.GetField();
        foreach (var c in oldField) {
            if (c != null) _graveyard.Enqueue(c);
        }
        var newField = DrawN(GameConfig.CARDS_IN_FIELD);
        cardSystem.SetField(newField);
        EventBus.Publish(new FieldChangedEvent { Field = cardSystem.GetField() });
    }

    /// <summary>드로우 풀 + 묘지 → 합쳐서 Fisher-Yates 셔플 → 새 드로우 풀.</summary>
    private void ShuffleAll() {
        var combined = new List<CardData>(_drawPile);
        while (_graveyard.Count > 0) combined.Add(_graveyard.Dequeue());
        // Fisher-Yates shuffle
        for (int i = combined.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            var tmp = combined[i]; combined[i] = combined[j]; combined[j] = tmp;
        }
        _drawPile.Clear();
        _drawPile.AddRange(combined);
    }

    private void ResetDrawPileFromDeck() {
        _drawPile.Clear();
        _drawPile.AddRange(_deck);
        // 초기 셔플
        for (int i = _drawPile.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            var tmp = _drawPile[i]; _drawPile[i] = _drawPile[j]; _drawPile[j] = tmp;
        }
    }

    // --- 테스트 전용 접근자 (public: 별도 assembly에서 접근; 프로덕션 코드에서 직접 호출 금지) ---
    public List<CardData>  DrawPile_TestOnly  => _drawPile;
    public Queue<CardData> Graveyard_TestOnly => _graveyard;
    public void ForceShuffleAll_TestOnly()    => ShuffleAll();
    public void TriggerKeyNote_TestOnly()     => OnKeyNote(new KeyNoteProcessedEvent());
}
