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
/// Sprint 1.5.2 T3: Bar 작은 점(0.5 scale, 푸른빛) + LaneAnchor 자식 Visual 띠 sprite.
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

        // Canvas 자식 — Score (Lo-Fi 우상단 좌측 — Canvas RefRes 956×440 기준 (-128, -8) from top-right)
        var scoreGO = CreateTMPText(canvasGO.transform, "Score", "0", 28);
        var scoreTmp = scoreGO.GetComponent<TextMeshProUGUI>();
        scoreTmp.alignment = TextAlignmentOptions.Right;
        var scoreRT = scoreGO.GetComponent<RectTransform>();
        scoreRT.anchorMin = new Vector2(1f, 1f);
        scoreRT.anchorMax = new Vector2(1f, 1f);
        scoreRT.pivot     = new Vector2(1f, 1f);
        scoreRT.anchoredPosition = new Vector2(-128f, -8f);
        scoreRT.sizeDelta = new Vector2(123f, 79f);

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

        // Canvas 자식 — Combo (Lo-Fi 우상단 우측 — Score 옆에 나란히, (-4, -8) from top-right)
        var comboGO = CreateTMPText(canvasGO.transform, "Combo", "", 32);
        var comboTmp = comboGO.GetComponent<TextMeshProUGUI>();
        comboTmp.alignment = TextAlignmentOptions.Right;
        var comboRT = comboGO.GetComponent<RectTransform>();
        comboRT.anchorMin = new Vector2(1f, 1f);
        comboRT.anchorMax = new Vector2(1f, 1f);
        comboRT.pivot     = new Vector2(1f, 1f);
        comboRT.anchoredPosition = new Vector2(-4f, -8f);
        comboRT.sizeDelta = new Vector2(123f, 79f);

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

        // Sprint 1.5.3 Lo-Fi placeholder: 일시정지 + 곡 정보
        // Sprint 1.5.4 fix: ⏸ (U+23F8) → "PAUSE" (MalgunGothic에 pause 심볼 glyph 없음)
        CreatePlaceholderPanel(
            canvasGO.transform, "PausePlaceholder",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(8f, -8f), new Vector2(77f, 79f),
            "PAUSE", new Color(1.0f, 0.95f, 0.4f, 0.7f),
            24
        );
        CreatePlaceholderPanel(
            canvasGO.transform, "SongInfoPlaceholder",
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),
            new Vector2(8f, -243f), new Vector2(129f, 119f),
            "곡 정보", new Color(0.85f, 0.85f, 0.85f, 0.6f),
            20
        );

        // Sprint 1.5.3 Lo-Fi placeholder: 셔플 + 덱묘지
        CreatePlaceholderPanel(
            canvasGO.transform, "ShufflePlaceholder",
            new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f),
            new Vector2(8f, 8f), new Vector2(80f, 79f),
            "셔플", new Color(1.0f, 0.95f, 0.4f, 0.7f),
            20
        );
        CreatePlaceholderPanel(
            canvasGO.transform, "DeckGravePlaceholder",
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0f),
            new Vector2(-8f, 8f), new Vector2(83f, 79f),
            "덱묘지", new Color(1.0f, 0.95f, 0.4f, 0.7f),
            18
        );

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
            bar.transform.localScale = new Vector3(0.5f, 0.5f, 1f);   // 작은 점 marker (Sprint 1.5.2 T3; BAR_LOCAL_SCALE=3.2 은 미사용)
            bar.tag = "Bar";
            var sr = bar.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.85f, 0.9f, 1.0f, 0.85f);   // 약한 푸른빛 흰색, 더 불투명
            sr.sortingOrder = 10;
            var barComp = bar.AddComponent<Bar>();
            barComp.barNum = n;
        }
        noteScreen.bars.Clear();
        noteScreen.bars.AddRange(noteScreenGO.GetComponentsInChildren<Bar>());
        // LaneAnchor 24개 자동 생성 — 입력 collider, full lane width 1.0
        // Sprint 1.5.2 T3: 자식 "Visual" GO에 띠 sprite 분리 (collider scale 영향 없음)
        for (int i = 0; i < 24; i++) {
            var laneGO = new GameObject($"LaneAnchor_{i}");
            laneGO.transform.SetParent(noteScreenGO.transform, false);
            laneGO.transform.localPosition = new Vector3(GameConfig.LaneX(i), 0f, 0f);
            // LaneAnchor root: collider만 — localScale 변경 없음 (collider world size 보존)
            var laneCol = laneGO.AddComponent<BoxCollider>();
            laneCol.center = new Vector3(0f, GameConfig.BAR_COLLIDER_CENTER_Y, 0f);
            laneCol.size = new Vector3(GameConfig.LANE_WIDTH, GameConfig.BAR_COLLIDER_SIZE_Y, GameConfig.BAR_COLLIDER_SIZE_Z);
            var laneComp = laneGO.AddComponent<LaneAnchor>();
            laneComp.laneIndex = i;
            // gameLoop 와이어링은 GameLoop 생성 후 아래서 수행

            // 자식 Visual — sprite (input collider와 독립 scale)
            var visualGO = new GameObject("Visual");
            visualGO.transform.SetParent(laneGO.transform, false);
            visualGO.transform.localPosition = new Vector3(0f, GameConfig.BAR_COLLIDER_CENTER_Y, 0f);  // collider 중심 y와 동일
            visualGO.transform.localScale = new Vector3(GameConfig.LANE_WIDTH, GameConfig.BAR_COLLIDER_SIZE_Y, 1f);
            var laneSR = visualGO.AddComponent<SpriteRenderer>();
            laneSR.sprite = sprite;
            laneSR.color = new Color(0.5f, 0.55f, 0.65f, 0.08f);   // 회색-푸른빛, 매우 투명 (노트 가독성 우선)
            // NOTE: 이 수치는 LaneAnchor.cs NORMAL_COLOR 와 동기화 필요 (T4 hover 복귀색).
            // 두 파일에 중복 정의 — spec 결정 전까지 수동 동기화.
            laneSR.sortingOrder = 5;   // Bar(10)보다 낮음 — background
            laneComp.visual = laneSR;   // T4 hover 토글용 사전 와이어링
        }
        noteScreen.lanes.Clear();
        noteScreen.lanes.AddRange(noteScreenGO.GetComponentsInChildren<LaneAnchor>());

        // Sprint 1.5.3 T4 fix: World Space visual band (lane/card area와 자동 정렬)
        // Canvas overlay 좌표가 화면 비율 의존 → World Space SpriteRenderer로 이전
        // NoteScreen localPosition.y = JUDGE_LINE_Y (-2.5) → 자식 localPosition은 world y에서 JUDGE_LINE_Y를 뺀 값
        CreateWorldVisualBand(noteScreenGO.transform, "NoteAreaBand",
            center: new Vector3(0f, (GameConfig.SPAWN_Y + GameConfig.JUDGE_LINE_Y) * 0.5f - GameConfig.JUDGE_LINE_Y, 0f),
            size: new Vector2(GameConfig.LANE_COUNT * GameConfig.LANE_WIDTH, GameConfig.SPAWN_Y - GameConfig.JUDGE_LINE_Y),
            color: new Color(1.0f, 0.95f, 0.4f, 0.05f),
            sortingOrder: 1,
            sprite: sprite);
        // JudgeLineBand: world y = JUDGE_LINE_Y → local y = 0 (NoteScreen root와 동일)
        CreateWorldVisualBand(noteScreenGO.transform, "JudgeLineBand",
            center: new Vector3(0f, 0f, 0f),
            size: new Vector2(GameConfig.LANE_COUNT * GameConfig.LANE_WIDTH, 0.3f),
            color: new Color(1.0f, 0.85f, 0.2f, 0.6f),
            sortingOrder: 6,
            sprite: sprite);
        // CardAreaBand: world y = -5 → local y = -5 - JUDGE_LINE_Y = -5 - (-2.5) = -2.5
        CreateWorldVisualBand(noteScreenGO.transform, "CardAreaBand",
            center: new Vector3(0f, -5f - GameConfig.JUDGE_LINE_Y, 0f),
            size: new Vector2(GameConfig.LANE_COUNT * GameConfig.LANE_WIDTH, 4.5f),
            color: new Color(1.0f, 0.95f, 0.4f, 0.10f),
            sortingOrder: 0,
            sprite: sprite);

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
        gameLoop.autoPerfectDebug = false;   // Sprint 1.5.2 — 실 입력 검증 default
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

        // Sprint 1.5.1: wire CardBoardUI into GameLoop (hover signal target)
        var cardBoardGO = GameObject.Find("CardBoard");
        if (cardBoardGO != null) {
            var cardBoardUI = cardBoardGO.GetComponent<CardBoardUI>();
            if (cardBoardUI != null && gameLoop != null) gameLoop.cardBoardUI = cardBoardUI;
        }

        // Also wire CardSystem
        var cardSystemGO = GameObject.Find("CardSystem");
        if (cardSystemGO != null) {
            var cs = cardSystemGO.GetComponent<CardSystem>();
            if (cs != null && gameLoop != null) gameLoop.cardSystem = cs;
        }

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

        Debug.Log($"[SceneBootstrap] Game.unity 씬 셋업 완료 — 6 루트 + 25 Bar(점 0.5) + 24 LaneAnchor(Visual 자식 띠) + 3 World Space band (NoteArea/JudgeLine/CardArea, NoteScreen 자식) + UI 8종(PauseInfo 2 + Shuffle/DeckGrave 2 Canvas overlay) + GameLoop 와이어링 완료. Build settings에 등록됨. (Sprint 1.5.3 T4 fix: Canvas overlay → World Space band)");
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
        // 1. CardSystem at root (unchanged)
        var existingSys = GameObject.Find("CardSystem");
        if (existingSys == null) {
            var sysGO = new GameObject("CardSystem");
            sysGO.AddComponent<CardSystem>();
            Debug.Log("[SceneBootstrap] CardSystem GameObject created.");
        }

        // 2. Remove legacy CardBoard if it lives under main Canvas (pre-refactor location)
        if (canvasGO != null) {
            var legacyBoard = canvasGO.transform.Find("CardBoard");
            if (legacyBoard != null) {
                Object.DestroyImmediate(legacyBoard.gameObject);
                Debug.Log("[SceneBootstrap] Legacy CardBoard (Canvas child) removed.");
            }
        }

        // 3. CardBoardCanvas (World Space) at root — covers lane area, just below judge line
        var existingCanvas2 = GameObject.Find("CardBoardCanvas");
        if (existingCanvas2 == null) {
            var canvas2GO = new GameObject("CardBoardCanvas", typeof(RectTransform));
            canvas2GO.layer = LayerMask.NameToLayer("UI");
            var c2rt = (RectTransform)canvas2GO.transform;
            // Position: center x=0 (lane area is symmetric -12..+12), y below judge line (-2.5)
            // Card top edge should clear judge line — center y = -5 (top -2.75, bottom -7.25 for height 4.5)
            canvas2GO.transform.position = new Vector3(0f, -5f, 0f);
            c2rt.sizeDelta = new Vector2(GameConfig.LANE_COUNT * GameConfig.LANE_WIDTH, 4.5f);  // 24 x 4.5
            c2rt.pivot = new Vector2(0.5f, 0.5f);

            var canvas2 = canvas2GO.AddComponent<Canvas>();
            canvas2.renderMode = RenderMode.WorldSpace;
            canvas2.sortingOrder = 1;
            canvas2GO.AddComponent<CanvasScaler>();   // default world space settings
            canvas2GO.AddComponent<GraphicRaycaster>();
            Debug.Log("[SceneBootstrap] CardBoardCanvas (WorldSpace) created.");
        }

        // 4. CardBoard under CardBoardCanvas
        var cardBoardCanvasGO = GameObject.Find("CardBoardCanvas");
        if (cardBoardCanvasGO != null) {
            var existingBoard = cardBoardCanvasGO.transform.Find("CardBoard");
            if (existingBoard == null) {
                var boardGO = new GameObject("CardBoard", typeof(RectTransform));
                boardGO.transform.SetParent(cardBoardCanvasGO.transform, false);
                boardGO.layer = LayerMask.NameToLayer("UI");
                boardGO.AddComponent<CardBoardUI>();
                Debug.Log("[SceneBootstrap] CardBoard added under CardBoardCanvas.");
            }
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
        // Sprint 1.5.4: 한글 라벨 지원을 위해 MalgunGothic SDF 직접 할당 (fallback chain 의존 안 함)
        var koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MalgunGothic SDF.asset");
        if (koreanFont != null) tmp.font = koreanFont;
        return go;
    }

    /// <summary>
    /// Lo-Fi placeholder: Image (bgColor) + 중앙 라벨 TMP. anchor/pivot/size로 위치 결정.
    /// Sprint 1.5.3 — 향후 Hi-Fi 외관 교체 예정.
    /// </summary>
    static GameObject CreatePlaceholderPanel(
        Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
        Vector2 anchoredPos, Vector2 size,
        string label, Color bgColor, int fontSize
    ) {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot     = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        var img = go.AddComponent<Image>();
        img.color = bgColor;

        if (!string.IsNullOrEmpty(label)) {
            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            labelGO.layer = LayerMask.NameToLayer("UI");
            var lrt = (RectTransform)labelGO.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero;
            lrt.offsetMax = Vector2.zero;
            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = fontSize;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            // Sprint 1.5.4: 한글 라벨 지원
            var koreanFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/MalgunGothic SDF.asset");
            if (koreanFont != null) tmp.font = koreanFont;
        }
        return go;
    }

    /// <summary>
    /// World Space visual band — lane/card area boundary 표시. 1×1 sprite + localScale로 size 조절.
    /// </summary>
    static GameObject CreateWorldVisualBand(Transform parent, string name, Vector3 center, Vector2 size, Color color, int sortingOrder, Sprite sprite) {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = center;
        go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = color;
        sr.sortingOrder = sortingOrder;
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
