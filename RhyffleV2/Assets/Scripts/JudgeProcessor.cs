using UnityEngine;

public class JudgeProcessor : MonoBehaviour {
    /// <summary>
    /// 거리(world unit) → 판정 등급. JUDGMENT_SPEC.md §1 공식.
    /// 임계 = (1 + k·r) × h
    /// </summary>
    public static JudgmentGrade ComputeGrade(float distance, float h, float r) {
        float perfect = (1f + GameConfig.PERFECT_K * r) * h;
        float great   = (1f + GameConfig.GREAT_K   * r) * h;
        float good    = (1f + GameConfig.GOOD_K    * r) * h;
        float miss    = (1f + GameConfig.MISS_K    * r) * h;

        if (distance <= perfect) return JudgmentGrade.Perfect;
        if (distance <= great)   return JudgmentGrade.Great;
        if (distance <= good)    return JudgmentGrade.Good;
        if (distance <= miss)    return JudgmentGrade.Miss;
        return JudgmentGrade.Miss;
    }

    /// <summary>
    /// 등급 → 점수 배율. JUDGMENT_SPEC.md §3.
    /// </summary>
    public static float GetScoreMultiplier(JudgmentGrade grade) {
        return grade switch {
            JudgmentGrade.Perfect => GameConfig.PERFECT_MULT,
            JudgmentGrade.Great   => GameConfig.GREAT_MULT,
            JudgmentGrade.Good    => GameConfig.GOOD_MULT,
            JudgmentGrade.Miss    => GameConfig.MISS_MULT,
            JudgmentGrade.SpMiss  => GameConfig.MISS_MULT,
            _                     => 0f,
        };
    }

    /// <summary>
    /// lane에 가장 가까운 active 노트 + 거리 반환. 없으면 null.
    /// GameLoop가 lane press 이벤트마다 호출.
    /// </summary>
    public Note FindNearestNoteInLane(int lane, System.Collections.Generic.IList<Note> activeNotes, out float distance) {
        Note nearest = null;
        distance = float.MaxValue;
        float judgeY = GameConfig.JUDGE_LINE_Y;
        foreach (var note in activeNotes) {
            if (note == null || note.IsJudged) continue;
            if (!note.ContainsLane(lane)) continue;
            float d = Mathf.Abs(note.GetCenter().y - judgeY);
            if (d < distance) {
                distance = d;
                nearest = note;
            }
        }
        return nearest;
    }
}
