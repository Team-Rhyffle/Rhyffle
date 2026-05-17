using UnityEngine;

public class Note : MonoBehaviour {
    // === 인스턴스 상태 (Spawn 시점 set, Reset 시 clear) ===
    public NoteKind        Kind;
    public int             Position;     // chart step
    public int             Line;         // leftmost lane 0~23
    public int             Length;       // 가로 폭 (lane 개수)
    public FlickDirection? Direction;    // Flick 한정
    public bool            IsJudged;

    /// <summary>
    /// Spawn 시 호출. ChartData에서 값 받아 인스턴스 초기화.
    /// 시각 속성(sprite/scale/color)은 prefab에 baked, 여기서 안 건드림.
    /// </summary>
    public void Initialize(NoteKind kind, int position, int line, int length, FlickDirection? dir = null) {
        Kind      = kind;
        Position  = position;
        Line      = line;
        Length    = length;
        Direction = dir;
        IsJudged  = false;
    }

    /// <summary>
    /// 풀 반환 시 호출 (Sprint 1엔 Destroy 직전 호출되거나 비호출).
    /// 위치/판정 상태만 reset.
    /// </summary>
    public void ResetForPool() {
        Position = 0;
        Line     = 0;
        Length   = 0;
        Direction = null;
        IsJudged = false;
    }

    /// <summary>
    /// 곡 시간에 따라 노트 위치 갱신 (drop). GameLoop가 매 프레임 호출.
    /// </summary>
    public void UpdatePosition(float songTime, float bpm, float speed) {
        float noteTimeSec = Conductor.StepToTime(Position, bpm);
        float timeUntilHit = noteTimeSec - songTime;  // 양수 = 아직 안 도달
        // 위에서 아래로 떨어짐: y = JUDGE_LINE_Y + (timeUntilHit * speed * (SPAWN_Y - JUDGE_LINE_Y) / spawnLeadTimeSec)
        float spawnLeadTimeSec = Conductor.StepToTime(GameConfig.SPAWN_LEAD_STEPS, bpm);
        float yProgress = Mathf.Clamp01(timeUntilHit / spawnLeadTimeSec);
        float y = GameConfig.JUDGE_LINE_Y + yProgress * (GameConfig.SPAWN_Y - GameConfig.JUDGE_LINE_Y);
        float x = GameConfig.BarX(Line + 1) + (Length - 1) * GameConfig.LANE_WIDTH * 0.5f;
        transform.position = new Vector3(x, y, 0f);
    }

    /// <summary>
    /// 판정 거리 계산용 노트 중심 좌표 (world).
    /// </summary>
    public Vector2 GetCenter() {
        return new Vector2(transform.position.x, transform.position.y);
    }

    /// <summary>
    /// 이 노트가 lane을 덮는지.
    /// </summary>
    public bool ContainsLane(int lane) {
        return lane >= Line && lane < Line + Length;
    }
}
