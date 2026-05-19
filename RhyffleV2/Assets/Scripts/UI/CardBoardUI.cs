using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sprint 1.5 placeholder UI: 7 horizontal slots at the bottom of the canvas.
// Each slot is a gray Image with a small TMP label. Updates on FieldChangedEvent.
// Children are created procedurally in Awake — no prefab dependencies, no Figma reference yet.
public class CardBoardUI : MonoBehaviour {
    const float SLOT_WIDTH      = 80f;
    const float SLOT_HEIGHT     = 120f;
    const float SLOT_SPACING    = 6f;
    const float BOTTOM_PADDING  = 12f;
    static readonly Color EMPTY_COLOR  = new Color(0.4f, 0.4f, 0.4f, 0.6f);
    static readonly Color FILLED_COLOR = new Color(0.85f, 0.85f, 0.85f, 0.9f);

    private readonly List<Image>           _slotImages = new List<Image>();
    private readonly List<TextMeshProUGUI> _slotLabels = new List<TextMeshProUGUI>();
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

    void BuildSlots() {
        // Ensure self has a RectTransform
        var rt = (RectTransform)transform;
        // Stretch horizontally along bottom of parent canvas
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot     = new Vector2(0.5f, 0f);
        float totalWidth = GameConfig.CARDS_IN_FIELD * SLOT_WIDTH + (GameConfig.CARDS_IN_FIELD - 1) * SLOT_SPACING;
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
            srt.sizeDelta = new Vector2(SLOT_WIDTH, SLOT_HEIGHT);
            srt.anchoredPosition = new Vector2(i * (SLOT_WIDTH + SLOT_SPACING), 0f);

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
        }
    }

    void OnFieldChanged(FieldChangedEvent evt) {
        for (int i = 0; i < GameConfig.CARDS_IN_FIELD; i++) {
            CardData card = (evt.Field != null && i < evt.Field.Count) ? evt.Field[i] : null;
            if (card == null) {
                _slotImages[i].color = EMPTY_COLOR;
                _slotLabels[i].text = "";
            } else {
                _slotImages[i].color = FILLED_COLOR;
                _slotLabels[i].text = $"{card.Suit}\n{card.Rank}";
            }
        }
    }
}
