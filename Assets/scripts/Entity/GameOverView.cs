using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverView : MonoBehaviour
{
    public Text title, subtitle, score, grade, duration, health, damage;
    public Image accent;
    public Button retryButton;
    public CanvasGroup content;
    public string previewBattleScene = "Assets/Scenes/전투1.unity";
    private bool loading;

    private void Start()
    {
        var result = ScoreManager.GetOrCreate().LastResult;
        title.text = result == null ? "전투 결과" : result.Victory ? (result.ObjectiveVictory ? "우주선 탈출 성공!" : "보스 격파!") : "다음엔 할 수 있어!";
        subtitle.text = result == null ? "전투를 마치면 이곳에 기록이 표시됩니다" : result.BossName + (result.Victory ? "  /  VICTORY" : "  /  GAME OVER");
        score.text = result == null ? "—" : result.Score.ToString("N0");
        grade.text = result == null ? "?" : result.Grade;
        duration.text = result == null ? "—" : string.Format("{0:00}:{1:00}", (int)result.Duration / 60, (int)result.Duration % 60);
        health.text = result == null ? "—" : string.Format("{0:0.#} / {1:0.#}", result.Health, result.MaxHealth);
        damage.text = result == null ? "—" : result.BossDamage.ToString("0.#");
        accent.color = result != null && !result.Victory ? new Color32(229, 143, 132, 255) : new Color32(176, 210, 154, 255);
        retryButton.GetComponentInChildren<Text>().text = result == null ? "전투 시작  →" : "다시 도전  →";
        retryButton.interactable = Application.CanStreamedLevelBeLoaded(result?.BattleScenePath ?? previewBattleScene);
        retryButton.onClick.AddListener(Retry);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(retryButton.gameObject);
        StartCoroutine(Reveal());
    }

    private IEnumerator Reveal()
    {
        content.alpha = 0;
        for (float t = 0; t < 0.55f; t += Time.unscaledDeltaTime)
        {
            content.alpha = Mathf.SmoothStep(0, 1, t / 0.55f);
            yield return null;
        }
        content.alpha = 1;
    }

    public void Retry()
    {
        if (loading) return;
        string path = ScoreManager.GetOrCreate().LastResult?.BattleScenePath ?? previewBattleScene;
        if (!Application.CanStreamedLevelBeLoaded(path)) return;
        loading = true;
        retryButton.interactable = false;
        SceneManager.LoadSceneAsync(path);
    }
    private void OnDestroy() { retryButton.onClick.RemoveListener(Retry); }
}
