using System;
using System.Collections.Generic;

[Serializable]
public class ChartData {
    public List<NormalNoteData> NormalNotes = new List<NormalNoteData>();
    public List<HoldNoteData>   HoldNotes   = new List<HoldNoteData>();
    public List<SlideNoteData>  SlideNotes  = new List<SlideNoteData>();
    public List<FlickNoteData>  FlickNotes  = new List<FlickNoteData>();
}

[Serializable] public struct NormalNoteData {
    public int position;    // chart step (3 = 1/64 박자)
    public int line;        // leftmost lane, 0~23
    public int length;      // 가로 폭 (lane 개수)
}

[Serializable] public struct HoldNoteData {
    public int position;
    public int line;
    public int count;       // hold 체인 그룹 ID (같은 count = 한 hold)
    public int length;
}

[Serializable] public struct SlideNoteData {
    public int position;
    public int line;
    public int length;
}

[Serializable] public struct FlickNoteData {
    public int position;
    public int line;
    public int length;
    public int direction;   // 0=Up, 1=Right, 2=Down, 3=Left (시계방향)
}
