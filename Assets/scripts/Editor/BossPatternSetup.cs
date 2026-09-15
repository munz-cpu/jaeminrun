using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BossPatternSetup
{
    [MenuItem("Tools/Boss Patterns/Configure Current Battle")]
    public static void Configure()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        var player = UnityEngine.Object.FindAnyObjectByType<RunnerMovement>();
        var bossHealth = UnityEngine.Object.FindObjectsByType<EntityHealth>().FirstOrDefault(h => h.GetComponent<FloatingObject>() != null);
        if (player == null || bossHealth == null) throw new InvalidOperationException("Expected battle player and boss.");
        var scene = bossHealth.gameObject.scene;
        Directory.CreateDirectory("Docs/AI");
        if (!File.Exists("Docs/AI/BeforeBossLive.unity"))
            if (!EditorSceneManager.SaveScene(scene, "Docs/AI/BeforeBossLive.unity", true))
                throw new InvalidOperationException("Could not back up unsaved scene.");

        var cookieSprite = LoadSprite("634699ec3cac80043ab04f12e54be18c");
        var minionSprite = LoadSprite("d61c61e691c98e742baafcf4396ed63a");
        var cookie = AssetDatabase.LoadAssetAtPath<BossProjectile>("Assets/Prefabs/BossCookie.prefab");
        if (cookie == null)
        {
            var root = new GameObject("BossCookie");
            var sr = root.AddComponent<SpriteRenderer>();
            sr.sprite = cookieSprite;
            sr.sortingOrder = 5;
            float scale = 0.65f / cookieSprite.bounds.size.x;
            root.transform.localScale = Vector3.one * scale;
            root.AddComponent<CircleCollider2D>().isTrigger = true;
            root.AddComponent<Rigidbody2D>();
            root.AddComponent<BossProjectile>();
            cookie = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/BossCookie.prefab").GetComponent<BossProjectile>();
            UnityEngine.Object.DestroyImmediate(root);
        }
        var minion = AssetDatabase.LoadAssetAtPath<BossMinion>("Assets/Prefabs/BossMinion.prefab");
        if (minion == null)
        {
            var root = new GameObject("BossMinion");
            root.layer = bossHealth.gameObject.layer;
            var visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            var sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = minionSprite;
            sr.sortingOrder = 2;
            float scale = 1.7f / minionSprite.bounds.size.y;
            visual.transform.localScale = Vector3.one * scale;
            var sourceBounce = player.GetComponentInChildren<BouncyRun2D>();
            var bounce = visual.AddComponent<BouncyRun2D>();
            if (sourceBounce != null) EditorUtility.CopySerialized(sourceBounce, bounce);
            bounce.isRunning = true;
            var col = root.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = (Vector2)minionSprite.bounds.size * scale;
            col.offset = (Vector2)minionSprite.bounds.center * scale;
            root.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
            root.AddComponent<EntityHealth>().maxHealth = 5f;
            root.AddComponent<BossMinion>();
            minion = PrefabUtility.SaveAsPrefabAsset(root, "Assets/Prefabs/BossMinion.prefab").GetComponent<BossMinion>();
            UnityEngine.Object.DestroyImmediate(root);
        }
        var boss = bossHealth.GetComponent<BossPatern1>();
        if (boss == null) boss = Undo.AddComponent<BossPatern1>(bossHealth.gameObject);

        var cookieSettings = new SerializedObject(cookie);
        cookieSettings.FindProperty("damage").floatValue = 1f;
        cookieSettings.ApplyModifiedProperties();
        EditorUtility.SetDirty(cookie);

        var playerHealth = player.GetComponent<EntityHealth>();
        if (playerHealth == null)
        {
            playerHealth = Undo.AddComponent<EntityHealth>(player.gameObject);
            playerHealth.maxHealth = 5f;
            EditorUtility.SetDirty(playerHealth);
        }

        Undo.RecordObject(boss, "Configure boss patterns");
        boss.player = player;
        boss.arenaCamera = Camera.main;
        boss.cookiePrefab = cookie;
        boss.minionPrefab = minion;
        var movement = new SerializedObject(player);
        minion.groundLayer = movement.FindProperty("groundLayer").intValue;
        EditorUtility.SetDirty(minion);
        movement.FindProperty("impactHitLayer").intValue |= 1 << minion.gameObject.layer;
        movement.ApplyModifiedProperties();
        EditorUtility.SetDirty(boss);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Boss setup complete: 3 patterns, prefab references, 5 HP minions, slam layer. Live scene backup: Docs/AI/BeforeBossLive.unity");
    }
    private static Sprite LoadSprite(string guid)
    {
        return AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GUIDToAssetPath(guid)).OfType<Sprite>().First();
    }
}
