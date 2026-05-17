using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HoldNoteBody : MonoBehaviour {
    public List<Note> Keypoints = new List<Note>();
    public int CurrentKeypointIndex;
    public bool IsBroken;

    private LineRenderer line;

    void Awake() {
        line = GetComponent<LineRenderer>();
    }

    /// <summary>
    /// HoldNote 그룹의 모든 keypoint 받아 폴리라인 시각화 초기화.
    /// </summary>
    public void Initialize(List<Note> sortedKeypoints) {
        Keypoints = sortedKeypoints;
        CurrentKeypointIndex = 0;
        IsBroken = false;
        line.positionCount = Keypoints.Count;
    }

    /// <summary>
    /// 매 프레임 keypoint 위치들로 line 갱신. GameLoop가 호출.
    /// </summary>
    public void UpdateLine(float songTime, float bpm, float speed) {
        for (int i = 0; i < Keypoints.Count; i++) {
            Keypoints[i].UpdatePosition(songTime, bpm, speed);
            line.SetPosition(i, Keypoints[i].transform.position);
        }
    }

    /// <summary>
    /// 사이판정 노트 1개 판정 처리 → CurrentKeypointIndex 진행.
    /// 8 중 4 받고 끊기면 5번째부터 fail 처리는 GameLoop/JudgeProcessor 협업.
    /// </summary>
    public void AdvanceKeypoint() {
        CurrentKeypointIndex++;
    }
}
