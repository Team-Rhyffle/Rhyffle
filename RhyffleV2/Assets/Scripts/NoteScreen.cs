using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bar 25 점 marker + LaneAnchor 24 입력 collider 컨테이너.
/// Pause 시 LaneAnchor 콜라이더 비활성화 → 입력 무시.
/// </summary>
public class NoteScreen : MonoBehaviour {
    public List<Bar>         bars  = new List<Bar>(25);
    public List<LaneAnchor>  lanes = new List<LaneAnchor>(24);

    void Awake() {
        if (bars.Count == 0)  bars.AddRange(GetComponentsInChildren<Bar>());
        if (lanes.Count == 0) lanes.AddRange(GetComponentsInChildren<LaneAnchor>());
    }

    /// <summary>
    /// Pause 시 LaneAnchor 콜라이더 비활성화 → 입력 무시. Bar 자체엔 collider 없음.
    /// (메서드명 SetBarsActive는 GameLoop.Pause/Resume 호환 유지)
    /// </summary>
    public void SetBarsActive(bool active) {
        foreach (var lane in lanes) {
            var collider = lane.GetComponent<BoxCollider>();
            if (collider != null) collider.enabled = active;
        }
    }

    public Bar GetBar(int barNum) {
        foreach (var bar in bars) {
            if (bar.barNum == barNum) return bar;
        }
        return null;
    }

    public LaneAnchor GetLane(int laneIndex) {
        foreach (var lane in lanes) {
            if (lane.laneIndex == laneIndex) return lane;
        }
        return null;
    }
}
