#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResultValidation : MonoBehaviour
{
    private bool previousBackground;
    [UnityEditor.MenuItem("Tools/Battle Results/Run Play Mode Validation")]
    private static void Run()
    {
        if (!Application.isPlaying) throw new System.InvalidOperationException("Enter Play Mode in the result scene first.");
        if (FindAnyObjectByType<BattleResultValidation>() != null) return;
        bool previous = Application.runInBackground;
        Application.runInBackground = true;
        new GameObject("Battle Result Validation").AddComponent<BattleResultValidation>().previousBackground = previous;
    }

    private IEnumerator Start()
    {
        DontDestroyOnLoad(gameObject);
        var view = FindAnyObjectByType<GameOverView>();
        Require(view != null && view.retryButton.interactable, "Direct result scene supports starting battle");
        view.retryButton.onClick.Invoke();
        yield return WaitForScene("전투1");
        yield return null;
        var flow = FindAnyObjectByType<BattleEndController>();
        Require(ScoreManager.Instance.LastResult == null, "New battle resets previous result");
        float max = flow.bossHealth.maxHealth;
        flow.bossHealth.TakeDamage(max + 1000);
        var victory = ScoreManager.Instance.LastResult;
        Require(flow.IsEnding && victory.Victory && victory.BossDamage == max, "Lethal boss damage saves victory and caps overkill");
        Require(Time.timeScale == 0, "Combat pauses during fade");
        flow.playerHealth.TakeDamage(flow.playerHealth.maxHealth);
        Require(ReferenceEquals(victory, ScoreManager.Instance.LastResult), "Duplicate death cannot replace saved result");
        yield return WaitForScene("전투 끝");
        yield return new WaitForSecondsRealtime(0.8f);
        view = FindAnyObjectByType<GameOverView>();
        Require(view.title.text == "보스 격파!" && view.score.text == victory.Score.ToString("N0"), "Victory result survives scene load and renders");
        Require(Time.timeScale == 1 && FindObjectsByType<ScoreManager>().Length == 1, "Time scale restored and singleton unique");
        view.retryButton.onClick.Invoke();
        yield return WaitForScene("전투1");
        yield return null;
        flow = FindAnyObjectByType<BattleEndController>();
        Require(ScoreManager.Instance.LastResult == null && !flow.IsEnding, "Retry starts a fresh battle");
        flow.bossHealth.TakeDamage(10);
        flow.playerHealth.TakeDamage(flow.playerHealth.maxHealth);
        Require(!ScoreManager.Instance.LastResult.Victory && ScoreManager.Instance.LastResult.Score == 100, "Defeat preserves earned damage points without victory bonus");
        yield return WaitForScene("전투 끝");
        yield return new WaitForSecondsRealtime(0.8f);
        view = FindAnyObjectByType<GameOverView>();
        Require(view.title.text == "다음엔 할 수 있어!" && view.grade.text == "D", "Defeat screen displays correct outcome");
        System.IO.File.WriteAllText("Docs/AI/BattleResultValidation.txt", "PASS: preview/start, victory, overkill, fade pause, duplicate death, scene persistence, singleton uniqueness, time restoration, retry reset, defeat score and UI. Unity Play Mode.");
        Debug.Log("BATTLE RESULT VALIDATION PASSED");
        Destroy(gameObject);
    }

    private static IEnumerator WaitForScene(string name)
    {
        float deadline = Time.realtimeSinceStartup + 15;
        while (SceneManager.GetActiveScene().name != name && Time.realtimeSinceStartup < deadline) yield return null;
        Require(SceneManager.GetActiveScene().name == name, "Scene transition: " + name);
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new System.Exception("Battle result validation failed: " + message);
        Debug.Log("PASS: " + message);
    }
    private void OnDestroy() { Application.runInBackground = previousBackground; }
}
#endif
