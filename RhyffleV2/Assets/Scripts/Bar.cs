using UnityEngine;

/// <summary>
/// 단일 lane anchor. SpriteRenderer는 placeholder (enabled=true + 1×1 white).
/// BoxCollider 역할 = 터치 입력 lane 식별 only. 판정은 JudgeProcessor가
/// 거리 기반으로 별도 계산 (콜라이더 hit과 무관). 콜라이더 y=9.6 unit은
/// lane 전체 입력 영역 확보용 — 노트 trajectory cover 목적 아님.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class Bar : MonoBehaviour {
    public int barNum;        // 1~24
    public GameLoop gameLoop; // 인스펙터 와이어링

    /// <summary>
    /// barNum → world x. 중심정렬 24 lane: Bar_1=-11.5, Bar_24=+11.5.
    /// </summary>
    public float GetWorldX() => GameConfig.BarX(barNum);
}
