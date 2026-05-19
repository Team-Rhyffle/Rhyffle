using UnityEngine;

/// <summary>
/// Lane 입력 collider 전담. 24 인스턴스 (laneIndex 0~23) — Bar 25 점 marker와 분리.
/// BoxCollider size_x = LANE_WIDTH (1.0). 두 Bar 사이 띠 영역 cover.
/// Sprint 1.5.1 도입 — Bar 24개 점 + lane 23개 띠 인식 문제 해결.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class LaneAnchor : MonoBehaviour {
    public int laneIndex;       // 0~23
    public GameLoop gameLoop;   // 인스펙터 와이어링
}
