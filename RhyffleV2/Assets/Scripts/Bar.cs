using UnityEngine;

/// <summary>
/// Lane 경계점 visual marker. 25 인스턴스 (barNum 1~25, lane 24개의 양 끝 + 사이).
/// 입력 책임은 LaneAnchor가 담당 (Sprint 1.5.1).
/// SpriteRenderer는 placeholder (enabled=true + 1×1 white, transparency로 점 표시).
/// </summary>
public class Bar : MonoBehaviour {
    public int barNum;        // 1~25

    /// <summary>
    /// barNum → world x. 중심정렬 25 marker: Bar_1=-12, Bar_13=0, Bar_25=+12.
    /// </summary>
    public float GetWorldX() => GameConfig.BarX(barNum);
}
