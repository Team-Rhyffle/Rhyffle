using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 24 Bar 컨테이너 + Bar 활성 토글 관리 (council 결정).
/// Sprint 1 단계엔 단순 컨테이너 + 자식 Bar 캐시. Pause 시 Bar 콜라이더 비활성화 가능.
/// </summary>
public class NoteScreen : MonoBehaviour {
    public List<Bar> bars = new List<Bar>(GameConfig.LANE_COUNT);

    void Awake() {
        // 자식 Bar 모두 캐시 (인스펙터 와이어링 누락 시 fallback)
        if (bars.Count == 0) {
            bars.AddRange(GetComponentsInChildren<Bar>());
        }
    }

    /// <summary>
    /// Pause 시 모든 Bar 콜라이더 비활성화 → 입력 무시.
    /// </summary>
    public void SetBarsActive(bool active) {
        foreach (var bar in bars) {
            var collider = bar.GetComponent<BoxCollider>();
            if (collider != null) collider.enabled = active;
        }
    }

    public Bar GetBar(int barNum) {
        foreach (var bar in bars) {
            if (bar.barNum == barNum) return bar;
        }
        return null;
    }
}
