#if UNITY_EDITOR
using System.Collections; using System.Collections.Generic; using System.IO; using UnityEditor; using UnityEngine;
public class BossPatternValidationRunner : MonoBehaviour
{
    private readonly List<string> results = new List<string>();
    private readonly HashSet<BossProjectile> cookies = new HashSet<BossProjectile>();
    private BossPatern1 boss;
    private bool sawSwing, sawWarning, sawMinions, sawReturn, sawLeft, sawRight;
    private float minSwingY = float.PositiveInfinity;
    private void Check(bool condition, string name)
    {
        results.Add((condition ? "PASS: " : "FAIL: ") + name);
    }
    private void Update()
    {
        if (boss == null) return;
        if (!sawSwing)
            foreach (var item in UnityEngine.Object.FindObjectsByType<BossProjectile>()) cookies.Add(item);
        if (boss.CurrentPattern == "Swing")
        {
            sawSwing = true;
            sawWarning |= GameObject.Find("Swing Warning") != null;
            minSwingY = Mathf.Min(minSwingY, boss.transform.position.y);
            float center = boss.arenaCamera.transform.position.x;
            sawLeft |= boss.transform.position.x < center - 6f;
            sawRight |= boss.transform.position.x > center + 6f;
        }
        sawMinions |= boss.MinionsSpawning && UnityEngine.Object.FindObjectsByType<BossMinion>().Length >= 1;
        sawReturn |= sawMinions && sawSwing && boss.CurrentPattern == "Cookies";
    }
    private IEnumerator Start()
    {
        boss = UnityEngine.Object.FindAnyObjectByType<BossPatern1>();
        Check(boss != null && boss.player != null && boss.cookiePrefab != null && boss.minionPrefab != null, "Scene references");
        if (boss == null) yield break;
        var start = new Vector2(6f, 4f);
        var target = new Vector2(-2f, -1f);
        var velocity = BossProjectile.AimVelocity(start, target, 1.3f, 18f);
        var landing = start + velocity*1.3f + Vector2.down*0.5f*18f*1.3f*1.3f;
        Check(Vector2.Distance(landing, target) < 0.001f, "Ballistic aim reaches sampled player position");
        var minion = Instantiate(boss.minionPrefab, new Vector3(100f, 0f, 0f), Quaternion.identity);
        minion.enabled = false;
        Check(minion.Health.CurrentHealth == 5f, "Minion starts at 5 HP");
        var popcorn = AssetDatabase.LoadAssetAtPath<Projectile>(AssetDatabase.GUIDToAssetPath("42d372d51771f2348a238f5f657de4f8"));
        for (int i = 1; i <= 5; i++)
        {
            if (minion == null) break;
            var shot = Instantiate(popcorn, new Vector3(98.5f, 0f, 0f), Quaternion.identity);
            shot.Launch(Vector2.right, 12f, 1f);
            yield return new WaitForSeconds(0.25f);
            if (i < 5) Check(minion != null && minion.Health.CurrentHealth == 5-i, "Popcorn physics hit " + i);
            else Check(minion == null, "Fifth popcorn destroys minion");
        }
        var slamTarget = Instantiate(boss.minionPrefab, new Vector3(100f, 0f, 0f), Quaternion.identity);
        slamTarget.HitBySlam();
        yield return null;
        Check(slamTarget == null, "One slam destroys full-health minion");
        var moving = Instantiate(boss.minionPrefab, new Vector3(100f, 0f, 0f), Quaternion.identity);
        float initialX = moving.transform.position.x;
        yield return new WaitForSeconds(0.5f);
        Check(moving.transform.position.x > initialX + 0.5f, "Minion moves left to right");
        Check(moving.GetComponentInChildren<BouncyRun2D>() != null && moving.GetComponent<BouncyRun2D>() == null, "Bounce isolated to visual child");
        Destroy(moving.gameObject);
        while (!sawReturn && Time.timeSinceLevelLoad < 60f) yield return null;
        Check(cookies.Count == 5, "First volley contains exactly five cookies (observed " + cookies.Count + ")");
        Check(sawSwing && sawWarning && sawLeft && sawRight && minSwingY < boss.swingBottomY+0.4f, "Warning and U-shaped traversal across both sides");
        Check(sawMinions && sawReturn, "Minion pattern and next cycle execute");
        boss.enabled = false;
        yield return null;
        Check(UnityEngine.Object.FindObjectsByType<BossProjectile>().Length == 0 && UnityEngine.Object.FindObjectsByType<BossMinion>().Length == 0 && GameObject.Find("Swing Warning") == null, "Disable cleans up spawned hazards and warning");
        string outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/AI"));
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllLines(Path.Combine(outputDirectory, "BossValidation.txt"), results);
        Debug.Log(string.Join("\n", results));
        EditorApplication.isPlaying = false;
    }
}


#endif
