using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sprint 1.5.1 — 8 slots sized to match the 3-lane world area (x=-12 to +12).
// BuildSlots() converts lane-area world edges to canvas local px via Camera+RectTransformUtility,
// then sets slotWidth = laneAreaCanvasPx / CARDS_IN_FIELD (≈ 645/8 ≈ 80.6 px at 16:9).
// This replaces the previous static CANVAS_REF_WIDTH/CARDS_IN_FIELD = 119.5 px calculation
// which was 4.4× wider than the actual lane column width.  (Sprint 1.5.1 follow-up patch)
// OnLaneHover() provides N-frame highlight tied to LaneToCardIndex.
// NOTE: sizing is computed once in Awake; screen-resize at runtime won't auto-update (placeholder ok).
// Children are created procedurally in Awake — no prefab dependencies, no Figma reference yet.
public class CardBoardUI : MonoBehaviour {
    // Slot dimensions: 1/N of lane-area canvas width (dynamically computed in BuildSlots).
    // At 16:9 / 956×440 reference: lane area ≈ 645 px, slotWidth ≈ 645/8 ≈ 80.6 px.
    const float SLOT_HEIGHT     = 120f;
    const float SLOT_SPACING    = 0f;          // 0 — slots flush; spacing is subsumed into slotWidth
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
        // --- Step 1: resolve canvas and camera ---
        var canvas = GetComponentInParent<Canvas>();
        var cam    = canvas != null ? canvas.worldCamera : null;
        if (cam == null) cam = Camera.main;

        // --- Step 2: lane-area world-edge points ---
        // Lane area is symmetric: x = -laneAreaHalfWidth … +laneAreaHalfWidth
        float laneAreaHalfWidth = (GameConfig.LANE_COUNT * GameConfig.LANE_WIDTH) * 0.5f;  // 24*1.0*0.5 = 12
        Vector3 worldLeft  = new Vector3(-laneAreaHalfWidth, 0f, 0f);   // (-12, 0, 0)
        Vector3 worldRight = new Vector3(+laneAreaHalfWidth, 0f, 0f);   // (+12, 0, 0)

        // --- Step 3: world → screen → canvas local px ---
        RectTransform canvasRT = canvas != null ? (canvas.transform as RectTransform) : null;
        Vector2 screenLeft  = cam != null ? (Vector2)cam.WorldToScreenPoint(worldLeft)  : Vector2.zero;
        Vector2 screenRight = cam != null ? (Vector2)cam.WorldToScreenPoint(worldRight) : Vector2.zero;
        Vector2 canvasLeft  = Vector2.zero;
        Vector2 canvasRight = Vector2.zero;
        if (canvasRT != null) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenLeft,  cam, out canvasLeft);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screenRight, cam, out canvasRight);
        }
        float totalWidth = Mathf.Abs(canvasRight.x - canvasLeft.x);   // lane area width in canvas px
        // Fallback if camera/canvas not yet ready (keeps previous broken size rather than zero)
        if (totalWidth < 1f) totalWidth = GameConfig.CANVAS_REF_WIDTH;
        float boardCenterX = (canvasLeft.x + canvasRight.x) * 0.5f;   // should be ≈0 for centred lane area

        // --- Step 4: position and size the board ---
        var rt = (RectTransform)transform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        rt.sizeDelta         = new Vector2(totalWidth, SLOT_HEIGHT);
        rt.anchoredPosition  = new Vector2(boardCenterX, BOTTOM_PADDING);

        float slotWidth = totalWidth / (float)GameConfig.CARDS_IN_FIELD;

        for (int i = 0; i < GameConfig.CARDS_IN_FIELD; i++) {
            var slot = new GameObject($"Slot_{i}", typeof(RectTransform));
            slot.transform.SetParent(transform, false);
            slot.layer = LayerMask.NameToLayer("UI");
            var srt = (RectTransform)slot.transform;
            srt.anchorMin = new Vector2(0f, 0.5f);
            srt.anchorMax = new Vector2(0f, 0.5f);
            srt.pivot     = new Vector2(0f, 0.5f);
            srt.sizeDelta        = new Vector2(slotWidth, SLOT_HEIGHT);
            srt.anchoredPosition = new Vector2(i * slotWidth, 0f);  // SLOT_SPACING stays 0

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
