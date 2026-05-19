#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Sprint 1 Game.unity 씬 풀 부트스트랩 + v1 prefab missing-script fix.
/// 메뉴 순서:
///   1. Rhyffle/Add Bar Tag (Tag 'Bar' 추가, SceneSetup.cs)
///   2. Rhyffle/Fix Note Prefabs (5 prefab missing script 제거 + Note 컴포넌트 + FlickNote variant 회전)
///   3. Rhyffle/Bootstrap Game Scene (새 Game.unity 생성, 6 루트 셋업, 25 Bar visual marker + 24 LaneAnchor collider 자동 생성, 인스펙터 와이어링)
/// Sprint 1.5.1: Bar 25 점 marker (입력 책임 없음) + LaneAnchor 24 입력 collider (1.0 width, full lane).
/// 모든 작업은 council 결정 + v1 인스펙션 실측값을 그대로 따름.
/// </summary>
public static class SceneBootstrap {
    const string SCENE_PATH = "Assets/Scenes/Game.unity";

    // ============================================================
    // 1. Fix Note Prefabs — v1 wrapper script 제거 + Note 컴포넌트 attach + FlickNote 회전
    // ============================================================

    [MenuItem("Rhyffle/Fix Note Prefabs")]
    static void FixNotePrefabs() {
        var paths = new[] {
            "Assets/Resources/Prefabs/Note/BasicNote.prefab",
            "Assets/Resources/Prefabs/Note/SlideNote.prefab",
            "Assets/Resources/Prefabs/Note/FlickNote.prefab",
            "Assets/Resources/Prefabs/Note/FlickNote_Up.prefab",
            "Assets/Resources/Prefabs/Note/FlickNote_Right.prefab",
            "Assets/Resources/Prefabs/Note/FlickNote_Down.prefab",
            "Assets/Resources/Prefabs/Note/FlickNote_Left.prefab",
            "Assets/Resources/Prefabs/Note/HoldNote.prefab",
            "Assets/Resources/Prefabs/Note/HoldNoteBody.prefab",
        };
        int fixedCount = 0;
        foreach (var path in paths) {
            var go = PrefabUtility.LoadPrefabContents(path);
            if (go == null) {
                Debug.LogWarning($"[SceneBootstrap] Prefab not found: {path}");
                continue;
            }
            // 1) Missing scripts 제거 (v1 wrapper script refs) — root + 자식 재귀
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            foreach (var t in go.GetComponentsInChildren<Transform>(true)) {
                if (t.gameObject == go) continue;
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
            }
            // 2) Note 컴포넌트 없으면 attach (HoldNoteBody는 별도 처리)
            if (path.Contains("HoldNoteBody")) {
                if (go.GetComponent<HoldNoteBody>() == null) go.AddComponent<HoldNoteBody>();
                if (go.GetComponent<LineRenderer>() == null) go.AddComponent<LineRenderer>();
            } else {
                if (go.GetComponent<Note>() == null) go.AddComponent<Note>();
            }
            // 3) FlickNote variant 회전 baked (Z축 회전 — 시계방향 0/-90/180/90)
            if (path.EndsWith("FlickNote_Up.prefab"))    go.transform.localRotation = Quaternion.Euler(0, 0, 0);
            if (path.EndsWith("FlickNote_Right.prefab")) go.transform.localRotation = Quaternion.Euler(0, 0, -90);
            if (path.EndsWith("FlickNote_Down.prefab"))  go.transform.localRotation = Quaternion.Euler(0, 0, 180);
            if (path.EndsWith("FlickNote_Left.prefab"))  go.transform.localRotation = Quaternion.Euler(0, 0, 90);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            PrefabUtility.UnloadPrefabContents(go);
            Debug.Log($"[SceneBootstrap] Fixed {path} (removed {removed} missing scripts)");
            fixedCount++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SceneBootstrap] Note prefabs fixed: {fixedCount}");
    }

    // ============================================================
    // 2. Bootstrap Full Game Scene
    // ============================================================

    [MenuItem("Rhyffle/Bootstrap Game Scene")]
    static void BootstrapGameScene() {
        // 디렉토리 보장
        if (!Directory.Exists("Assets/Scenes")) {
            Directory.CreateDirectory("Assets/Scenes");
            AssetDatabase.Refresh();
        }
        // Tag 'Bar' 보장
        EnsureTagExists("Bar");

        // 새 씬 생성 (기존 Game.unity 덮어쓰기)
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // === 1. Main Camera ===
        var cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        cameraGO.transform.position = new Vector3(0f, GameConfig.CAMERA_POS_Y, GameConfig.CAMERA_POS_Z);
        var cam = cameraGO.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = GameConfig.CAMERA_ORTHO_SIZE;
        cam.nearClipPlane = 0.3f;
        cam.farClipPlane = 1000f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(49f / 255f, 77f / 255f, 121f / 255f, 0f);
        cameraGO.AddComponent<AudioListener>();
        cameraGO.AddComponent<PhysicsRaycaster>();  // Council 결정 #2: World BoxCollider hit raycast
        cameraGO.AddComponent<CameraAspectFit>();   // Steelman Scenario C: 화면 aspect 무관 24 lane fit

        // === 2. Canvas ===
        var canvasGO = new GameObject("Canvas", typeof(RectTransform));
        canvasGO.layer = LayerMask.NameToLayer("UI");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = cam;
        canvas.planeDistance = GameConfig.CANVAS_PLANE_DISTANCE;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(GameConfig.CANVAS_REF_WIDTH, GameConfig.CANVAS_REF_HEIGHT);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        scaler.matchWidthOrHeight = 0f;
        scaler.referencePixelsPerUnit = 100f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Canvas 자식 — Score (좌측중앙)
        var scoreGO = CreateTMPText(canvasGO.transform, "Score", "0", 30);
        var scoreRT = scoreGO.GetComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(0f, 0.5f);
        scoreRT.anchorMax = new Vector2(0f, 0.5f);
        scoreRT.pivot     = new Vector2(0f, 0.5f);
        scoreRT.anchoredPosition = new Vector2(25f, 0f);
        scoreRT.sizeDelta = new Vector2(200f, 50f);

        // Canvas 자식 — JudgementText (중앙 stretch)
        var judgeGO = CreateTMPText(canvasGO.transform, "JudgementText", "", 60);
        var judgeTmp = judgeGO.GetComponent<TextMeshProUGUI>();
        judgeTmp.fontStyle = FontStyles.Bold;
        judgeTmp.alignment = TextAlignmentOptions.Center;
        var judgeRT = judgeGO.GetComponent<RectTransform>();
        judgeRT.anchorMin = new Vector2(0f, 0f);
        judgeRT.anchorMax = new Vector2(1f, 1f);
        judgeRT.pivot     = new Vector2(0.5f, 0.5f);
        judgeRT.anchoredPosition = new Vector2(0f, -50f);
        judgeRT.offsetMin = new Vector2(200f, 150f);
        judgeRT.offsetMax = new Vector2(-200f, -250f);

        // Canvas 자식 — Combo (중앙 상단, council 결정)
        var comboGO = CreateTMPText(canvasGO.transform, "Combo", "", 50);
        var comboTmp = comboGO.GetComponent<TextMeshProUGUI>();
        comboTmp.alignment = TextAlignmentOptions.Center;
        var comboRT = comboGO.GetComponent<RectTransform>();
        comboRT.anchorMin = new Vector2(0.5f, 1f);
        comboRT.anchorMax = new Vector2(0.5f, 1f);
        comboRT.pivot     = new Vector2(0.5f, 1f);
        comboRT.anchoredPosition = new Vector2(0f, -50f);
        comboRT.sizeDelta = new Vector2(200f, 80f);

        // Canvas 자식 — PauseCountdown Image (중앙, Sprint 1엔 disable)
        var pauseGO = new GameObject("PauseCountdown", typeof(RectTransform));
        pauseGO.transform.SetParent(canvasGO.transform, false);
        pauseGO.layer = LayerMask.NameToLayer("UI");
        var pauseImg = pauseGO.AddComponent<Image>();
        var pauseSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Resources/Art/UI/Pause/1.png");
        if (pauseSprite != null) pauseImg.sprite = pauseSprite;
        var pauseRT = pauseGO.GetComponent<RectTransform>();
        pauseRT.anchorMin = new Vector2(0.5f, 0.5f);
        pauseRT.anchorMax = new Vector2(0.5f, 0.5f);
        pauseRT.sizeDelta = new Vector2(200f, 200f);
        pauseGO.SetActive(false);

        // === 3. EventSystem ===
        var eventSystemGO = new GameObject("EventSystem");
        eventSystemGO.AddComponent<EventSystem>();
        eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // === 4. GameSystem (컨테이너) ===
        var gameSystemGO = new GameObject("GameSystem");
        gameSystemGO.transform.position = Vector3.zero;

        // 4-1. NoteScreen (Bar 25개 + LaneAnchor 24개 부모)
        var noteScreenGO = new GameObject("NoteScreen");
        noteScreenGO.transform.SetParent(gameSystemGO.transform, false);
        noteScreenGO.transform.localPosition = new Vector3(0f, GameConfig.JUDGE_LINE_Y, 0f);
        var noteScreen = noteScreenGO.AddComponent<NoteScreen>();
        // Bar 25개 자동 생성 — visual marker only (no BoxCollider, no gameLoop wiring)
        // Sprint 1.5.1: 입력 책임은 LaneAnchor 24개로 분리
        var sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        if (sprite == null) sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        for (int n = 1; n <= 25; n++) {
            var bar = new GameObject($"Bar_{n}");
            bar.transform.SetParent(noteScreenGO.transform, false);
            bar.transform.localPosition = new Vector3(GameConfig.BarX(n), 0f, 0f);
            bar.transform.localScale = new Vector3(GameConfig.BAR_LOCAL_SCALE, GameConfig.BAR_LOCAL_SCALE, 1f);
            bar.tag = "Bar";
            var sr = bar.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, 0.3f);
            sr.sortingOrder = 10;
            var barComp = bar.AddComponent<Bar>();
            barComp.barNum = n;
        }
        noteScreen.bars.Clear();
        noteScreen.bars.AddRange(noteScreenGO.GetComponentsInChildren<Bar>());
        // LaneAnchor 24개 자동 생성 — 입력 collider, full lane width 1.0
        for (int i = 0; i < 24; i++) {
            var laneGO = new GameObject($"LaneAnchor_{i}");
            laneGO.transform.SetParent(noteScreenGO.transform, false);
            laneGO.transform.localPosition = new Vector3(GameConfig.LaneX(i), 0f, 0f);
            var laneCol = laneGO.AddComponent<BoxCollider>();
            laneCol.center = new Vector3(0f, GameConfig.BAR_COLLIDER_CENTER_Y, 0f);
            laneCol.size = new Vector3(GameConfig.LANE_WIDTH, GameConfig.BAR_COLLIDER_SIZE_Y, GameConfig.BAR_COLLIDER_SIZE_Z);
            var laneComp = laneGO.AddComponent<LaneAnchor>();
            laneComp.laneIndex = i;
            // gameLoop 와이어링은 GameLoop 생성 후 아래서 수행
        }
        noteScreen.lanes.Clear();
        noteScreen.lanes.AddRange(noteScreenGO.GetComponentsInChildren<LaneAnchor>());

        // 4-2. NoteBoard (hold polyline 공유 LineRenderer)
        var noteBoardGO = new GameObject("NoteBoard");
        noteBoardGO.transform.SetParent(gameSystemGO.transform, false);
        noteBoardGO.transform.localPosition = Vector3.zero;
        noteBoardGO.AddComponent<LineRenderer>();

        // === 5. Audio Source (+ Conductor 컴포넌트) ===
        var audioGO = new GameObject("Audio Source");
        audioGO.transform.position = Vector3.zero;
        var audio = audioGO.AddComponent<AudioSource>();
        audio.volume = 0.3f;
        audio.pitch = 1f;
        audio.spatialBlend = 0f;
        audio.playOnAwake = false;
        audio.loop = false;
        var conductor = audioGO.AddComponent<Conductor>();
        conductor.bpm = GameConfig.DEFAULT_BPM;
        conductor.offset = GameConfig.DEFAULT_OFFSET;

        // === 6. GameLoop (orchestrator + JudgeProcessor 동거) ===
        var gameLoopGO = new GameObject("GameLoop");
        var judgeProcessor = gameLoopGO.AddComponent<JudgeProcessor>();
        var gameLoop = gameLoopGO.AddComponent<GameLoop>();
        gameLoop.conductor = conductor;
        gameLoop.noteScreen = noteScreen;
        gameLoop.judgeProcessor = judgeProcessor;
        gameLoop.scoreText = scoreGO.GetComponent<TextMeshProUGUI>();
        gameLoop.comboText = comboGO.GetComponent<TextMeshProUGUI>();
        gameLoop.judgementText = judgeTmp;
        gameLoop.chartName = "dummy_test";

        // Note prefab 5종 + FlickNote 4방향 wiring
        gameLoop.basicNotePrefab      = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Note/BasicNote.prefab");
        gameLoop.slideNotePrefab      = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Note/SlideNote.prefab");
        gameLoop.flickNoteUpPrefab    = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Note/FlickNote_Up.prefab");
        gameLoop.flickNoteRightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Note/FlickNote_Right.prefab");
        gameLoop.flickNoteDownPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Note/FlickNote_Down.prefab");
        gameLoop.flickNoteLeftPrefab  = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Note/FlickNote_Left.prefab");
        gameLoop.holdNotePrefab       = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Note/HoldNote.prefab");
        gameLoop.holdNoteBodyPrefab   = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Prefabs/Note/HoldNoteBody.prefab");

        // LaneAnchor gameLoop ref 와이어링 (Bar는 gameLoop 필드 없음 — Sprint 1.5.1)
        foreach (var lane in noteScreen.lanes) lane.gameLoop = gameLoop;

        // === 7. CardSystem + CardBoard (idempotent helper) ===
        EnsureCardSystemAndBoard(canvasGO);

        // === 씬 저장 ===
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Build Settings에 씬 등록
        var buildScenes = EditorBuildSettings.scenes;
        bool alreadyIn = false;
        foreach (var s in buildScenes) if (s.path == SCENE_PATH) { alreadyIn = true; break; }
        if (!alreadyIn) {
            var newScenes = new EditorBuildSettingsScene[buildScenes.Length + 1];
            for (int i = 0; i < buildScenes.Length; i++) newScenes[i] = buildScenes[i];
            newScenes[buildScenes.Length] = new EditorBuildSettingsScene(SCENE_PATH, true);
            EditorBuildSettings.scenes = newScenes;
        }

        Debug.Log($"[SceneBootstrap] Game.unity 씬 셋업 완료 — 6 루트 + 25 Bar + 24 LaneAnchor + UI 4종 + GameLoop 와이어링 완료. Build settings에 등록됨. (Sprint 1.5.1)");
    }

    // ============================================================
    // 3. Add CardSystem + CardBoard to Current Scene (idempotent)
    // ============================================================

    [MenuItem("Rhyffle/Add CardSystem+CardBoard to Current Scene")]
    static void AddCardSystemAndBoardMenu() {
        var canvasGO = GameObject.Find("Canvas");
        if (canvasGO == null) {
            Debug.LogError("[SceneBootstrap] Canvas not found in current scene.");
            return;
        }
        EnsureCardSystemAndBoard(canvasGO);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        AssetDatabase.SaveAssets();
    }

    // ============================================================
    // Helpers
    // ============================================================

    static void EnsureCardSystemAndBoard(GameObject canvasGO) {
        // CardSystem: top-level GameObject
        var existingSys = GameObject.Find("CardSystem");
        if (existingSys == null) {
            var sysGO = new GameObject("CardSystem");
            sysGO.AddComponent<CardSystem>();
            Debug.Log("[SceneBootstrap] CardSystem GameObject created.");
        }
        // CardBoard: child of Canvas
        Transform existingBoard = canvasGO.transform.Find("CardBoard");
        if (existingBoard == null) {
            var boardGO = new GameObject("CardBoard", typeof(RectTransform));
            boardGO.transform.SetParent(canvasGO.transform, false);
            boardGO.layer = LayerMask.NameToLayer("UI");
            boardGO.AddComponent<CardBoardUI>();
            Debug.Log("[SceneBootstrap] CardBoard added under Canvas.");
        }
    }

    static GameObject CreateTMPText(Transform parent, string name, string text, int fontSize) {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        return go;
    }

    static void EnsureTagExists(string tagName) {
        var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tagsProp = tagManager.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++) {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tagName) return;
        }
        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tagName;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"[SceneBootstrap] Tag '{tagName}' added.");
    }
}
#endif
