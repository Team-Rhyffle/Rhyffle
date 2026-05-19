using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sprint 1.5.1 World Space Canvas refactor — CardBoard now lives under a dedicated
// CardBoardCanvas (World Space) whose coordinate space matches lane GameObjects directly
// (x = -12 to +12 world units).  BuildSlots() uses static world-unit sizing — no camera
// conversion, no RectTransformUtility gymnastics, Free Aspect safe.
// slotWidth = LANES_PER_CARD * LANE_WIDTH = 3 world units; SLOT_HEIGHT = 4.5 world units (≈ 2:3 ratio).
// OnLaneHover() provides N-frame highlight tied to LaneToCardIndex.
// Children are created procedurally in Awake — no prefab dependencies, no Figma reference yet.
public class CardBoardUI : MonoBehaviour {
    // Slot dimensions in world units — CardBoardCanvas is World Space so these map 1:1.
    const float SLOT_HEIGHT     = 4.5f;   // world units — 3:4.5 (≈ 2:3 poker ratio)
    const float SLOT_SPACING    = 0f;     // world units — slots flush
    const int   HOVER_FRAMES    = 12;     // ~0.2s @ 60fps
    static readonly Color EMPTY_COLOR  = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    static readonly Color FILLED_COLOR = new Color(0.85f, 0.85f, 0.85f, 0.9f);
    static readonly Color HOVER_COLOR  = new Color(1.0f, 0.9f, 0.4f, 0.95f);   // 노란 톤

    private readonly List<Image>           _slotImages = new List<Image>();
    private readonly List<TextMeshProUGUI> _slotLabels = new List<TextMeshProUGUI>();
    private readonly List<int>             _hoverFramesRemaining = new List<int>();   // parallel to slots
    private System.Action<FieldChangedEvent> _handler;

    void Awake() {
        BuildSlots();
    }
    void OnEnable() {
        _handler = OnFieldChanged;
        EventBus.Subscribe(_handler);
    }
    void OnDisable() {
        if (_handler != null) EventBus.Unsubscribe(_handler);
        _handler = null;
    }

    void Update() {
        for (int i = 0; i < _hoverFramesRemaining.Count; i++) {
            if (_hoverFramesRemaining[i] > 0) {
                _hoverFramesRemaining[i]--;
                if (_hoverFramesRemaining[i] == 0) {
                    // Restore color based on field state (label empty → EMPTY, otherwise FILLED).
                    _slotImages[i].color = string.IsNullOrEmpty(_slotLabels[i].text) ? EMPTY_COLOR : FILLED_COLOR;
                }
            }
        }
    }

    // Called by GameLoop.HandlePress (click) and GameLoop.UpdateHoverRaycast (mouse-over).
    // laneIndex < 0 means "clear hover" — guard first to avoid C# -1/3==0 division quirk.
    // Highlights the corresponding card slot for HOVER_FRAMES frames.
    public void OnLaneHover(int laneIndex) {
        if (laneIndex < 0 || laneIndex >= GameConfig.LANE_COUNT) return;
        int cardIdx = GameConfig.LaneToCardIndex(laneIndex);
        if (cardIdx < 0 || cardIdx >= _slotImages.Count) return;
        _slotImages[cardIdx].color = HOVER_COLOR;
        _hoverFramesRemaining[cardIdx] = HOVER_FRAMES;
    }

    void BuildSlots() {
        var rt = (RectTransform)transform;
        float slotWidth  = GameConfig.LANES_PER_CARD * GameConfig.LANE_WIDTH;   // 3 world units
        float totalWidth = GameConfig.CARDS_IN_FIELD * slotWidth;               // 24 world units
        // CardBoard fills its parent (CardBoardCanvas) — anchor center, sizeDelta = lane area span.
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta        = new Vector2(totalWidth, SLOT_HEIGHT);
        rt.anchoredPosition = new Vector2(0f, 0f);

        for (int i = 0; i < GameConfig.CARDS_IN_FIELD; i++) {
            var slot = new GameObject($"Slot_{i}", typeof(RectTransform));
            slot.transform.SetParent(transform, false);
            var srt = (RectTransform)slot.transform;
            // Anchor at parent's left-middle; pivot at slot's own left-middle.
            // anchoredPosition.x = i * slotWidth ⇒ slot i covers parent-local x = [i*3 .. (i+1)*3], offset from left edge.
            srt.anchorMin = new Vector2(0f, 0.5f);
            srt.anchorMax = new Vector2(0f, 0.5f);
            srt.pivot     = new Vector2(0f, 0.5f);
            srt.sizeDelta        = new Vector2(slotWidth, SLOT_HEIGHT);
            srt.anchoredPosition = new Vector2(i * slotWidth, 0f);

            var img = slot.AddComponent<Image>();
            img.color = EMPTY_COLOR;
            _slotImages.Add(img);
            _hoverFramesRemaining.Add(0);

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(slot.transform, false);
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(0.15f, 0.15f);    // world unit padding
            lrt.offsetMax = new Vector2(-0.15f, -0.15f);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 1.0f;   // world unit ~ 1/20 of camera view height (≈ 5% screen height)
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            _slotLabels.Add(tmp);
        }
    }

    void OnFieldChanged(FieldChangedEvent evt) {
        for (int i = 0; i < GameConfig.CARDS_IN_FIELD; i++) {
            CardData card = (evt.Field != null && i < evt.Field.Count) ? evt.Field[i] : null;
            if (card == null) {
                if (_hoverFramesRemaining[i] == 0) _slotImages[i].color = EMPTY_COLOR;
                _slotLabels[i].text = "";
            } else {
                if (_hoverFramesRemaining[i] == 0) _slotImages[i].color = FILLED_COLOR;
                _slotLabels[i].text = $"{card.Suit}\n{card.Rank}";
            }
        }
    }
}
