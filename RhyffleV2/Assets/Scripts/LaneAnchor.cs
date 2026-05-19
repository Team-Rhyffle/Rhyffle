using UnityEngine;

/// <summary>
/// Lane 입력 collider 전담. 24 인스턴스 (laneIndex 0~23) — Bar 25 점 marker와 분리.
/// BoxCollider size_x = LANE_WIDTH (1.0). 두 Bar 사이 띠 영역 cover.
/// Sprint 1.5.1 도입 — Bar 24개 점 + lane 23개 띠 인식 문제 해결.
/// Sprint 1.5.2 T3: 자식 "Visual" SpriteRenderer 추가 (띠 outline placeholder).
/// Sprint 1.5.2 T4: SetHover() — hover 색 토글. NORMAL_COLOR 수치는
///   SceneBootstrap.cs line 223 laneSR.color 와 동일하게 유지할 것 (동기화 주의).
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class LaneAnchor : MonoBehaviour {
    public int laneIndex;           // 0~23
    public GameLoop gameLoop;       // 인스펙터 와이어링
    public SpriteRenderer visual;   // 자식 Visual의 SpriteRenderer (T4 hover 토글용)

    // NOTE: NORMAL_COLOR 수치는 SceneBootstrap.cs의 laneSR.color 초기값과 동기화 필요.
    // 두 파일에 중복 정의된 상태 — spec 결정 전까지 수동 동기화.
    static readonly Color NORMAL_COLOR = new Color(0.5f, 0.55f, 0.65f, 0.08f);
    static readonly Color HOVER_COLOR  = new Color(0.7f,  0.85f, 1.0f,  0.30f);

    /// <summary>hover 상태 시각 토글. visual null 이면 no-op.</summary>
    public void SetHover(bool on) {
        if (visual == null) return;
        visual.color = on ? HOVER_COLOR : NORMAL_COLOR;
    }
}
