using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class EscapeStageSetup
{
    [MenuItem("Tools/Escape Stage/Update Boss Cycle")]
    public static void UpdateBossCycle()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.name != "전투2")
            throw new System.InvalidOperationException("Open battle 2 in Edit Mode.");
        var boss = Object.FindAnyObjectByType<BossSlamHunter>();
        var stage = Object.FindAnyObjectByType<EscapeStageController>();
        if (!boss || !stage) throw new System.InvalidOperationException("Escape stage is missing.");
        Undo.RecordObject(boss, "Configure timed boss cycle");
        boss.waitingDuration = 4f;
        boss.warningDuration = 1f;
        boss.riseDuration = 0.5f;
        boss.exposedDuration = 1.5f;
        boss.fallDuration = 0.5f;
        boss.slamDuration = 0.1f;
        boss.visibleHeight = 8f;
        boss.scanDistance = 1.4f;
        boss.scanTilt = 5f;
        boss.cameraShake = Object.FindAnyObjectByType<CameraShake>();
        EditorUtility.SetDirty(boss);
        foreach (var cover in stage.houses)
        {
            if (!cover) continue;
            Undo.RecordObject(cover, "Match hut concealment to artwork");
            cover.useSpriteBounds = true;
            EditorUtility.SetDirty(cover);
        }
        if (stage.instructionText)
        {
            Undo.RecordObject(stage.instructionText, "Update escape instructions");
            stage.instructionText.text = "← → 이동  ·  진동이 오면 초가집 뒤에 숨기  ·  보스가 보는 동안 나오면 즉사";
            EditorUtility.SetDirty(stage.instructionText);
        }
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Tools/Escape Stage/Apply")]
    public static void Apply()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (Application.isPlaying || scene.name != "전투2") throw new System.InvalidOperationException("Open battle 2 in Edit Mode.");
        if (Object.FindAnyObjectByType<EscapeStageController>() != null) throw new System.InvalidOperationException("Escape stage already configured.");
        var player = Object.FindAnyObjectByType<RunnerMovement>();
        var ship = GameObject.Find("로켓_0").transform;
        var house = GameObject.Find("초가집_0");
        var bossObject = GameObject.Find("boss");
        var arm = GameObject.Find("팔");
        Undo.RegisterFullObjectHierarchyUndo(player.gameObject, "Configure escape player");
        var root = new GameObject("Escape Stage");
        Undo.RegisterCreatedObjectUndo(root, "Create escape stage");
        var flow = root.AddComponent<BattleEndController>();
        flow.playerHealth = player.GetComponent<EntityHealth>();
        flow.objectiveVictoryOnly = true;
        flow.bossDisplayName = "이서후 · 우주선 탈출";
        flow.playerHealth.maxHealth = 1;
        var healthData = new SerializedObject(flow.playerHealth);
        healthData.FindProperty("destroyThis").boolValue = false;
        healthData.ApplyModifiedProperties();
        var oldHealthBar = player.GetComponent<플레이어체력바>();
        if (oldHealthBar) { Undo.RecordObject(oldHealthBar, "Disable unused health bar"); oldHealthBar.enabled = false; }
        var stage = root.AddComponent<EscapeStageController>();
        stage.player = player; stage.playerHealth = flow.playerHealth; stage.result = flow; stage.ship = ship;
        var boss = Undo.AddComponent<BossSlamHunter>(bossObject);
        stage.boss = boss; boss.stage = stage;
        boss.cameraShake = Object.FindAnyObjectByType<CameraShake>();
        boss.arm = arm.transform; boss.armRenderer = arm.GetComponent<SpriteRenderer>();
        boss.bossRenderer = bossObject.GetComponent<SpriteRenderer>();
        Undo.RecordObject(arm.transform, "Resize attack arm");
        arm.transform.localScale *= 4.5f / boss.armRenderer.bounds.size.x;
        arm.transform.localRotation = Quaternion.Euler(0, 0, -135);
        boss.armRenderer.sortingOrder = 30;
        // Keep the authored start/end scenery, remove only arena constraints.
        var walls = GameObject.Find("좌우 벽");
        Undo.RecordObject(walls, "Disable arena walls"); walls.SetActive(false);
        var leftWall = new GameObject("Escape Left Boundary", typeof(BoxCollider2D));
        leftWall.transform.SetParent(root.transform);
        leftWall.transform.position = new Vector3(-8, 0, 0);
        leftWall.GetComponent<BoxCollider2D>().size = new Vector2(1, 40);
        leftWall.layer = 6;
        var existingBar = GameObject.Find("Canvas/bossbar");
        if (existingBar) { Undo.RecordObject(existingBar, "Hide boss health"); existingBar.SetActive(false); }
        var covers = new System.Collections.Generic.List<HutCover>();
        for (int i=0; i<11; i++)
        {
            var hut = i == 0 ? house : Object.Instantiate(house);
            if (i == 0) Undo.RegisterFullObjectHierarchyUndo(hut, "Place first cover"); else Undo.RegisterCreatedObjectUndo(hut, "Create cover");
            hut.name = "초가집 숨기 " + (i+1);
            hut.transform.position = new Vector3(15f+i*20f, -3.15f, 0);
            hut.transform.localScale = new Vector3(1.4f, 1.4f, 1);
            var sr = hut.GetComponent<SpriteRenderer>(); sr.sortingOrder = 25;
            var cover = hut.GetComponent<HutCover>();
            if (!cover) cover = hut.AddComponent<HutCover>();
            cover.size = new Vector2(5.5f, 3.8f); cover.offset = new Vector2(0, -0.1f);
            cover.useSpriteBounds = true;
            covers.Add(cover);
        }
        stage.houses = covers.ToArray();
        var marker = new GameObject("Slam Impact Warning", typeof(SpriteRenderer));
        marker.transform.SetParent(root.transform);
        var warning = marker.GetComponent<SpriteRenderer>();
        warning.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        warning.drawMode = SpriteDrawMode.Sliced;
        warning.size = new Vector2(boss.impactSize.x, 0.22f);
        warning.sortingOrder = 31; warning.color = Color.red; warning.enabled = false;
        boss.warning = warning;
        var canvasObject = new GameObject("Escape HUD", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(root.transform);
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObject.GetComponent<Canvas>().sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
        var font = AssetDatabase.FindAssets("t:Font", new[]{"Assets/fonts"}).Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Font>).First(f=>f!=null);
        stage.progressText = Label(canvasObject.transform, font, "Progress", "우주선까지  0%  →", 22, 28);
        stage.statusText = Label(canvasObject.transform, font, "Status", "오른쪽 우주선으로 이동하세요", 64, 26);
        stage.instructionText = Label(canvasObject.transform, font, "Instructions", "← → 이동  ·  진동이 오면 초가집 뒤에 숨기  ·  보스가 보는 동안 나오면 즉사", 105, 20);
        var scenes = EditorBuildSettings.scenes.ToList();
        if (!scenes.Any(s=>s.path==scene.path)) scenes.Add(new EditorBuildSettingsScene(scene.path, true));
        else scenes.First(s=>s.path==scene.path).enabled = true;
        EditorBuildSettings.scenes = scenes.ToArray();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("Escape stage configured and saved.");
    }

    private static Text Label(Transform parent, Font font, string name, string content, float top, int size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0,1); rect.pivot = new Vector2(0,1);
        rect.anchoredPosition = new Vector2(24,-top); rect.sizeDelta = new Vector2(760,40);
        go.GetComponent<Image>().color = new Color(0.05f,0.08f,0.12f,0.8f);
        var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(go.transform, false);
        var tr = textObject.GetComponent<RectTransform>(); tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one;
        tr.offsetMin=new Vector2(12,0); tr.offsetMax=new Vector2(-12,0);
        var text = textObject.GetComponent<Text>(); text.font=font; text.fontSize=size;
        text.text=content; text.color=Color.white; text.alignment=TextAnchor.MiddleLeft;
        text.raycastTarget=false; return text;
    }

    [MenuItem("Tools/Escape Stage/Inspect")]
    public static void Inspect()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        Directory.CreateDirectory("Docs/AI");
        File.WriteAllLines("Docs/AI/EscapeInspection.txt", scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Transform>(true)).Select(t => {
            var sr = t.GetComponent<SpriteRenderer>();
            var col = t.GetComponent<Collider2D>();
            return t.name + " pos=" + t.position + " scale=" + t.lossyScale + (sr ? " sprite=" + AssetDatabase.GetAssetPath(sr.sprite) + " bounds=" + sr.bounds + " order=" + sr.sortingOrder + " layer=" + sr.sortingLayerName : "") + (col ? " collider=" + col.bounds + " layer=" + t.gameObject.layer : "");
        }));
        if (!Application.isPlaying && !File.Exists("Docs/AI/BeforeEscapeStage.unity"))
            EditorSceneManager.SaveScene(scene, "Docs/AI/BeforeEscapeStage.unity", true);
    }
}
