using UnityEngine;

// Keeps the latest battle result across scene loads (session storage, not a disk save).
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public BattleResult LastResult { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() { Instance = null; }

    public static ScoreManager GetOrCreate()
    {
        if (Instance == null) new GameObject("ScoreManager").AddComponent<ScoreManager>();
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void BeginBattle() { LastResult = null; }
    public void SaveResult(BattleResult result) { LastResult = result; }
    private void OnDestroy() { if (Instance == this) Instance = null; }
}

public sealed class BattleResult
{
    public string BossName { get; }
    public string BattleScenePath { get; }
    public bool Victory { get; }
    public bool ObjectiveVictory { get; }
    public float Duration { get; }
    public float Health { get; }
    public float MaxHealth { get; }
    public float BossDamage { get; }
    public int Score { get; }
    public string Grade { get; }

    public BattleResult(string bossName, string scenePath, bool victory, float duration,
        float health, float maxHealth, float bossDamage, int score, bool objectiveVictory = false)
    {
        ObjectiveVictory = objectiveVictory;
        BossName = bossName; BattleScenePath = scenePath; Victory = victory;
        Duration = Mathf.Max(0, duration); Health = Mathf.Max(0, health);
        MaxHealth = Mathf.Max(1, maxHealth); BossDamage = Mathf.Max(0, bossDamage);
        Score = Mathf.Max(0, score);
        float ratio = Health / MaxHealth;
        Grade = !victory ? "D" : ratio >= 0.8f ? "S" : ratio >= 0.5f ? "A" : ratio >= 0.2f ? "B" : "C";
    }
}
