using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sprint 1.5.1 — 8 slots, total width = Canvas RefWidth (956 px).
// Each slot width = CANVAS_REF_WIDTH / CARDS_IN_FIELD = 956/8 = 119.5 px,
// matching 3-lane visual width (LANES_PER_CARD = 3).
// OnLaneHover() provides N-frame highlight tied to LaneToCardIndex.
// Canvas must use ScaleWithScreenSize anchored to RefWidth (956×440); the px value
// scales with screen at runtime, keeping slot-to-lane alignment intact.
// Children are created procedurally in Awake — no prefab dependencies, no Figma reference yet.
public class CardBoardUI : MonoBehaviour {
    // Slot dimensions: 1/N of Canvas reference width (N = CARDS_IN_FIELD = 8).
    // Currently 956/8 = 119.5 px per slot, matching 3-lane visual width.
    const float SLOT_HEIGHT     = 120f;
    const float SLOT_SPACING    = 0f;          // 0 — slots flush so 8×width = full canvas
    const float BOTTOM_PADDING  = 12f;
    const int   HOVER_FRAMES    = 12;          // ~0.2s @ 60fps
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

    // Called by GameLoop.HandlePress when user touches a lane.
    // Highlights the corresponding card slot for HOVER_FRAMES frames.
    public void OnLaneHover(int laneIndex) {
        int cardIdx = GameConfig.LaneToCardIndex(laneIndex);
        if (cardIdx < 0 || cardIdx >= _slotImages.Count) return;
        _slotImages[cardIdx].color = HOVER_COLOR;
        _hoverFramesRemaining[cardIdx] = HOVER_FRAMES;
    }

    void BuildSlots() {
        // Ensure self has a RectTransform
        var rt = (RectTransform)transform;
        // Stretch horizontally along bottom of parent canvas
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        // Slot width computed from Canvas reference resolution so all 8 slots fill the full canvas width.
        float slotWidth  = GameConfig.CANVAS_REF_WIDTH / (float)GameConfig.CARDS_IN_FIELD;
        float totalWidth = GameConfig.CARDS_IN_FIELD * slotWidth + (GameConfig.CARDS_IN_FIELD - 1) * SLOT_SPACING;  // = 956 with spacing 0
        rt.sizeDelta         = new Vector2(totalWidth, SLOT_HEIGHT);
        rt.anchoredPosition  = new Vector2(0f, BOTTOM_PADDING);

        for (int i = 0; i < GameConfig.CARDS_IN_FIELD; i++) {
            var slot = new GameObject($"Slot_{i}", typeof(RectTransform));
            slot.transform.SetParent(transform, false);
            slot.layer = LayerMask.NameToLayer("UI");
            var srt = (RectTransform)slot.transform;
            srt.anchorMin = new Vector2(0f, 0.5f);
            srt.anchorMax = new Vector2(0f, 0.5f);
            srt.pivot     = new Vector2(0f, 0.5f);
            srt.sizeDelta        = new Vector2(slotWidth, SLOT_HEIGHT);
            srt.anchoredPosition = new Vector2(i * (slotWidth + SLOT_SPACING), 0f);

            var img = slot.AddComponent<Image>();
            img.color = EMPTY_COLOR;
            _slotImages.Add(img);

            // Label inside the slot
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(slot.transform, false);
            labelGO.layer = LayerMask.NameToLayer("UI");
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(4f, 4f);
            lrt.offsetMax = new Vector2(-4f, -4f);
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = "";
            tmp.fontSize = 14;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            _slotLabels.Add(tmp);

            _hoverFramesRemaining.Add(0);
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
