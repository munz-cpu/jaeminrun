using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleEndController : MonoBehaviour
{
    [Header("Reusable battle configuration")]
    public EntityHealth playerHealth;
    public EntityHealth bossHealth;
    public bool objectiveVictoryOnly;
    public string bossDisplayName = "BOSS 01";
    public string resultScenePath = "Assets/Scenes/전투 끝.unity";
    [Min(0.1f)] public float fadeDuration = 1.2f;
    [Header("Score rules")]
    [Min(0)] public int pointsPerDamage = 10;
    [Min(0)] public int victoryBonus = 1000;
    [Min(0)] public int fullHealthBonus = 500;
    [Min(0)] public float timeBonusLimit = 120f;
    [Min(0)] public int pointsPerSecond = 5;
    public bool IsEnding { get; private set; }
    private float elapsed;
    private float damage;
    private float previousTimeScale = 1f;
    private bool ownsPause;

    private void Start()
    {
        if (playerHealth == null || (!objectiveVictoryOnly && bossHealth == null) ||
            !Application.CanStreamedLevelBeLoaded(resultScenePath))
        {
            Debug.LogError("BattleEndController requires player/boss health and an enabled result scene.", this);
            enabled = false;
            return;
        }
        ScoreManager.GetOrCreate().BeginBattle();
        playerHealth.Died += OnPlayerDied;
        if (!objectiveVictoryOnly)
        {
            bossHealth.Died += OnBossDied;
            bossHealth.Damaged += OnBossDamaged;
        }
    }

    private void Update()
    {
        if (IsEnding) return;
        elapsed += Time.deltaTime;
        // Also supports older scripts/Inspector edits that directly change currentHealth.
        if (playerHealth != null && playerHealth.IsDead) Finish(false);
        else if (!objectiveVictoryOnly && bossHealth != null && bossHealth.IsDead) Finish(true);
    }
    public void CompleteObjective()
    {
        if (objectiveVictoryOnly && playerHealth != null && !playerHealth.IsDead) Finish(true);
    }
    private void OnBossDamaged(float amount) { if (!IsEnding) damage += amount; }
    private void OnPlayerDied() { Finish(false); }
    private void OnBossDied() { Finish(playerHealth != null && !playerHealth.IsDead); }

    private void Finish(bool victory)
    {
        if (IsEnding) return;
        IsEnding = true;
        int score = Mathf.RoundToInt(damage * pointsPerDamage);
        if (victory)
            score += victoryBonus + Mathf.RoundToInt(fullHealthBonus * playerHealth.CurrentHealth / Mathf.Max(1, playerHealth.maxHealth))
                + Mathf.FloorToInt(Mathf.Max(0, timeBonusLimit - elapsed) * pointsPerSecond);
        ScoreManager.GetOrCreate().SaveResult(new BattleResult(bossDisplayName, gameObject.scene.path,
            victory, elapsed, playerHealth.CurrentHealth, playerHealth.maxHealth, damage, score, objectiveVictoryOnly));
        var input = playerHealth.GetComponent<PlayerInput>();
        if (input != null) input.DeactivateInput();
        // Freeze combat immediately; fade uses unscaled time and keeps working while paused.
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        ownsPause = true;
        StartCoroutine(FadeAndLoad());
    }

    private IEnumerator FadeAndLoad()
    {
        var overlay = new GameObject("Battle Fade", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        overlay.transform.SetParent(transform, false);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;
        var panel = new GameObject("Black", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(overlay.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var image = panel.GetComponent<Image>();
        image.color = Color.clear;
        for (float t = 0; t < fadeDuration; t += Time.unscaledDeltaTime)
        {
            image.color = new Color(0, 0, 0, Mathf.Clamp01(t / fadeDuration));
            yield return null;
        }
        image.color = Color.black;
        yield return null;
        yield return SceneManager.LoadSceneAsync(resultScenePath);
    }

    private void OnDestroy()
    {
        if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
        if (bossHealth != null) { bossHealth.Died -= OnBossDied; bossHealth.Damaged -= OnBossDamaged; }
        if (ownsPause) Time.timeScale = previousTimeScale;
    }
}
