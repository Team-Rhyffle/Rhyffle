using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Sprint 1.5.5 T1: 일시정지 버튼 click handler. PausePlaceholder에 attach.
// GameLoop.Pause/Resume 토글. 라벨 "PAUSE" ↔ "PLAY".
[RequireComponent(typeof(Button))]
public class PauseButton : MonoBehaviour {
    public GameLoop gameLoop;
    public TextMeshProUGUI label;

    private const string PAUSE_LABEL = "PAUSE";
    private const string PLAY_LABEL  = "PLAY";

    void Awake() {
        var btn = GetComponent<Button>();
        btn.onClick.AddListener(OnClick);
    }

    void Start() {
        if (gameLoop == null) gameLoop = FindObjectOfType<GameLoop>();
        if (label == null) label = GetComponentInChildren<TextMeshProUGUI>();
        UpdateLabel();
    }

    void OnClick() {
        if (gameLoop == null) return;
        if (gameLoop.IsPaused) gameLoop.Resume();
        else                   gameLoop.Pause();
        UpdateLabel();
    }

    void UpdateLabel() {
        if (label == null || gameLoop == null) return;
        label.text = gameLoop.IsPaused ? PLAY_LABEL : PAUSE_LABEL;
    }
}
