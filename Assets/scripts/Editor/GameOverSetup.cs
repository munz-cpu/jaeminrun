using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class GameOverSetup
{
    private static readonly Color Ink = new Color32(38, 48, 66, 255);
    private static readonly Color Cream = new Color32(247, 240, 222, 255);
    private static Font font;

    [MenuItem("Tools/Battle Results/Configure Scenes")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying) throw new System.InvalidOperationException("Stop Play Mode first.");
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                throw new System.InvalidOperationException("Save current scene edits before configuring.");
        var battle = EditorSceneManager.OpenScene("Assets/Scenes/전투1.unity");
        var player = Object.FindAnyObjectByType<RunnerMovement>();
        var boss = Object.FindAnyObjectByType<BossPatern1>();
        var flow = Object.FindAnyObjectByType<BattleEndController>();
        if (flow == null) flow = new GameObject("Battle End Controller").AddComponent<BattleEndController>();
        flow.playerHealth = player.GetComponent<EntityHealth>();
        flow.bossHealth = boss.GetComponent<EntityHealth>();
        flow.bossDisplayName = "재민 보스";
        var playerSprite = player.GetComponentInChildren<SpriteRenderer>().sprite;
        EditorSceneManager.SaveScene(battle);
        var results = EditorSceneManager.OpenScene("Assets/Scenes/전투 끝.unity");
        EnsureBuildScenes();
        if (Object.FindAnyObjectByType<GameOverView>() != null) return;
        font = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Resources/Fonts & Materials/SeoulNamsanB.ttf");
        var root = new GameObject("Battle Results", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(GameOverView));
        root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
        var view = root.GetComponent<GameOverView>();
        var bg = Box(root.transform, "Background", Vector2.zero, new Vector2(1280, 720), Ink);
        bg.rectTransform.anchorMin = Vector2.zero; bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.sizeDelta = Vector2.zero;
        var page = Box(root.transform, "Result Card", Vector2.zero, new Vector2(1120, 600), Cream);
        view.content = page.gameObject.AddComponent<CanvasGroup>();
        Box(page.transform, "Top Stripe", new Vector2(0, 295), new Vector2(1120, 10), new Color32(234, 170, 95, 255));
        Label(page.transform, "Brand", "JAEMIN RUN   /   BATTLE REPORT", -500, 254, 700, 24, 17, Ink);
        Label(page.transform, "Issue", "기록은 다음 도전을 위해", 275, 254, 240, 24, 15, Ink);
        Box(page.transform, "Divider", new Vector2(0, 220), new Vector2(1000, 2), Ink);
        view.title = Label(page.transform, "Title", "전투 결과", -500, 162, 710, 65, 48, Ink);
        view.subtitle = Label(page.transform, "Subtitle", "전투를 마치면 이곳에 기록이 표시됩니다", -498, 104, 700, 32, 19, Ink);
        view.accent = Box(page.transform, "Rank Badge", new Vector2(400, 105), new Vector2(180, 170), new Color32(176, 210, 154, 255));
        Label(view.accent.transform, "Rank Label", "RANK", -80, 54, 160, 24, 15, Ink, TextAnchor.MiddleCenter);
        view.grade = Label(view.accent.transform, "Rank", "?", -80, -5, 160, 95, 76, Ink, TextAnchor.MiddleCenter);
        Label(page.transform, "Score Label", "TOTAL SCORE  /  획득 점수", -498, 35, 600, 28, 16, Ink);
        view.score = Label(page.transform, "Score", "—", -500, -30, 670, 92, 76, Ink);
        if (playerSprite != null)
        {
            var portrait = Box(page.transform, "Player Portrait", new Vector2(255, -30), new Vector2(100, 95), Color.white);
            portrait.sprite = playerSprite; portrait.preserveAspect = true;
        }
        view.duration = Stat(page.transform, "Time", "전투 시간", -330);
        view.health = Stat(page.transform, "Health", "남은 체력", 0);
        view.damage = Stat(page.transform, "Damage", "보스에게 준 피해", 330);
        var buttonImage = Box(page.transform, "Retry", new Vector2(330, -238), new Vector2(340, 66), Ink);
        view.retryButton = buttonImage.gameObject.AddComponent<Button>();
        view.retryButton.targetGraphic = buttonImage;
        var colors = view.retryButton.colors; colors.highlightedColor = new Color(0.78f, 0.85f, 1); colors.selectedColor = colors.highlightedColor; view.retryButton.colors = colors;
        Label(buttonImage.transform, "Text", "전투 시작  →", -165, 0, 330, 55, 23, Cream, TextAnchor.MiddleCenter);
        Label(page.transform, "Footer", "한 번 더, 더 멀리.\n새로운 기록에 도전해 보세요.", -500, -238, 530, 58, 19, Ink);
        if (Object.FindAnyObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        EditorSceneManager.SaveScene(results);
        AssetDatabase.SaveAssets();
    }

    private static void EnsureBuildScenes()
    {
        var paths = new[] { "Assets/Scenes/전투1.unity", "Assets/Scenes/전투 끝.unity" };
        var scenes = EditorBuildSettings.scenes.ToList();
        foreach (var path in paths)
        {
            var entry = scenes.FirstOrDefault(s => s.path == path);
            if (entry == null) scenes.Add(new EditorBuildSettingsScene(path, true)); else entry.enabled = true;
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
    }

    private static Text Stat(Transform parent, string name, string label, float x)
    {
        var box = Box(parent, name, new Vector2(x, -142), new Vector2(315, 99), new Color32(231, 224, 207, 255));
        Label(box.transform, "Label", label, -137, 23, 275, 26, 16, Ink);
        return Label(box.transform, "Value", "—", -137, -16, 275, 43, 32, Ink);
    }
    private static Image Box(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>(); image.color = color;
        image.rectTransform.sizeDelta = size; image.rectTransform.anchoredPosition = position;
        return image;
    }
    private static Text Label(Transform parent, string name, string value, float x, float y, float width, float height, int size, Color color, TextAnchor alignment = TextAnchor.MiddleLeft)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<Text>(); text.font = font; text.text = value; text.fontSize = size;
        text.color = color; text.alignment = alignment; text.raycastTarget = false;
        text.rectTransform.sizeDelta = new Vector2(width, height);
        text.rectTransform.anchoredPosition = new Vector2(x + width / 2, y);
        return text;
    }
}
