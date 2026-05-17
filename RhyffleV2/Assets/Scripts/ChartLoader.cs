using UnityEngine;

public static class ChartLoader {
    /// <summary>
    /// Resources/Charts/{path}.json 로드 → ChartData
    /// </summary>
    public static ChartData Load(string path) {
        TextAsset textAsset = Resources.Load<TextAsset>($"Charts/{path}");
        if (textAsset == null) {
            Debug.LogError($"[ChartLoader] Chart not found: Charts/{path}");
            return new ChartData();
        }
        return LoadFromJson(textAsset.text);
    }

    /// <summary>
    /// JSON 문자열 → ChartData (테스트용 + 핵심 로직)
    /// </summary>
    public static ChartData LoadFromJson(string json) {
        return JsonUtility.FromJson<ChartData>(json);
    }
}
