#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Sprint 1 씬 셋업 1회성 Editor 메뉴. Game.unity 의 NoteScreen 자식으로 24 Bar 자동 생성.
/// council 결정: 중심정렬 N - 12.5, 라인 간격 1.0 unit, 3D BoxCollider, SpriteRenderer enabled 1×1 white α=0.3.
/// </summary>
public static class SceneSetup {
    [MenuItem("Rhyffle/Setup 24 Bars")]
    static void Setup24Bars() {
        var noteScreen = GameObject.Find("NoteScreen");
        if (noteScreen == null) {
            Debug.LogError("[SceneSetup] NoteScreen GameObject not found. Create it first under GameSystem.");
            return;
        }

        // 기존 Bar 제거 (재실행 안전)
        var existing = noteScreen.GetComponentsInChildren<Bar>();
        foreach (var b in existing) {
            Object.DestroyImmediate(b.gameObject);
        }

        // Unity built-in 1×1 white sprite — placeholder
        var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null) {
            sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        }

        for (int n = 1; n <= GameConfig.LANE_COUNT; n++) {
            var bar = new GameObject($"Bar_{n}");
            bar.transform.SetParent(noteScreen.transform, false);
            bar.transform.localPosition = new Vector3(GameConfig.BarX(n), 0f, 0f);
            bar.transform.localScale = new Vector3(GameConfig.BAR_LOCAL_SCALE, GameConfig.BAR_LOCAL_SCALE, 1f);
            bar.tag = "Bar";

            var sr = bar.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, 0.3f);
            sr.sortingOrder = 10;

            var col = bar.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, GameConfig.BAR_COLLIDER_CENTER_Y, 0f);
            col.size = new Vector3(GameConfig.BAR_COLLIDER_SIZE_X, GameConfig.BAR_COLLIDER_SIZE_Y, GameConfig.BAR_COLLIDER_SIZE_Z);

            var barComp = bar.AddComponent<Bar>();
            barComp.barNum = n;
        }

        // NoteScreen.bars 자동 갱신 (Awake fallback이 GetComponentsInChildren 사용)
        var ns = noteScreen.GetComponent<NoteScreen>();
        if (ns != null) {
            ns.bars.Clear();
            ns.bars.AddRange(noteScreen.GetComponentsInChildren<Bar>());
            EditorUtility.SetDirty(ns);
        }

        Debug.Log($"[SceneSetup] {GameConfig.LANE_COUNT} Bars created under NoteScreen.");
    }

    [MenuItem("Rhyffle/Add Bar Tag (if missing)")]
    static void AddBarTag() {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");

        // 이미 있나 확인
        for (int i = 0; i < tagsProp.arraySize; i++) {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == "Bar") {
                Debug.Log("[SceneSetup] Tag 'Bar' already exists.");
                return;
            }
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = "Bar";
        tagManager.ApplyModifiedProperties();
        Debug.Log("[SceneSetup] Tag 'Bar' added.");
    }
}
#endif
