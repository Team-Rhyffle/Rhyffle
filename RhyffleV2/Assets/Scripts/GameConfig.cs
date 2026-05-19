public static class GameConfig {
    // === Lane / Spawn 좌표 (v1 인스펙션 실측 + council 결정) ===
    public const int   LANE_COUNT       = 24;
    public const float LANE_WIDTH       = 1.0f;    // v1 21 lane Bar 간격 실측
    public const float JUDGE_LINE_Y     = -2.5f;   // v1 Bar world y 실측
    public const float SPAWN_Y          = 7.75f;   // v1 출발선 y
    public const float NOTE_HEIGHT      = 1.0f;    // h, Hi-Fi 도착 시 갱신
    // === 시간 (채보 step + BPM) ===
    public const int   STEPS_PER_QUARTER = 48;     // 1/4박자 = 48 step (CHART_SPEC.md)
    public const int   SPAWN_LEAD_STEPS  = 96;     // 노트 도착 2박자 전 spawn
    public const float DEFAULT_BPM       = 120f;   // Sprint 1 하드코드
    public const float DEFAULT_OFFSET    = 0f;
    public const float DEFAULT_SPEED     = 1.0f;   // r — Sprint 1 옵션 노출 X

    // === 카메라 / Canvas (v1 인스펙션 실측) ===
    public const float CAMERA_ORTHO_SIZE = 10.0f;
    public const float CAMERA_POS_Y      = 1.5f;
    public const float CAMERA_POS_Z      = -10.0f;
    public const int   CANVAS_REF_WIDTH  = 956;
    public const int   CANVAS_REF_HEIGHT = 440;
    public const float CANVAS_PLANE_DISTANCE = 100f;

    // === 판정 윈도우 계수 (h 단위, JUDGMENT_SPEC.md) ===
    public const float PERFECT_K = 0.2f;
    public const float GREAT_K   = 0.4f;
    public const float GOOD_K    = 0.7f;
    public const float MISS_K    = 1.0f;

    // === 점수 배율 (만점 1.2M Plain Mode) ===
    public const float PERFECT_MULT = 1.2f;
    public const float GREAT_MULT   = 1.0f;
    public const float GOOD_MULT    = 0.7f;
    public const float MISS_MULT    = 0.0f;

    // === Bar 콜라이더 (v1 실측 — lane 입력 식별 only, 판정 무관) ===
    public const float BAR_COLLIDER_CENTER_Y = -0.84f;
    public const float BAR_COLLIDER_SIZE_X   = 0.32f;  // Sprint 1.5.1+ 사용 안 함 — Hi-Fi marker 도입 시 활용 예정
    public const float BAR_COLLIDER_SIZE_Y   = 3.0f;
    public const float BAR_COLLIDER_SIZE_Z   = 0.2f;
    public const float BAR_LOCAL_SCALE       = 3.2f;

    // === Bar / Lane 좌표 헬퍼 (Sprint 1.5.1+) ===
    public const int BAR_COUNT = 25;     // lane 24개의 양 끝 + 사이 marker 25 (중심정렬)
    // BarX: lane 경계점 (점 marker) world x. 25 marker.
    // LaneX: lane 영역 (띠) center world x. 24 lane.
    public static float BarX(int barNum) => barNum - 13f;
    public static float LaneX(int laneIndex) => laneIndex - 11.5f;

    // === Card System (Sprint 1.5+) ===
    public const int CARDS_IN_FIELD          = 7;   // 필드 카드 수 (spec §4 + Lo-Fi UI 7 슬롯 동일)
    public const int KEY_NOTE_COMBO_INTERVAL = 32;  // 키 노트 트리거 콤보 간격 (임의 값 — 플레이테스트 미검증, 2박 단위 어림셈 수준; spec 미정 — stub, Sprint 2 갱신)
}

public enum NoteKind { Normal, Hold, Slide, Flick }
public enum FlickDirection { Up = 0, Right = 1, Down = 2, Left = 3 }  // 시계방향
public enum JudgmentGrade { Perfect, Great, Good, Miss, SpMiss }       // SpMiss = HoldNote 마지막 떼기 (v1 salvage)
