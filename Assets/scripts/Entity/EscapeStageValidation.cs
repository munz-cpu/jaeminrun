#if UNITY_EDITOR
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeStageValidation : MonoBehaviour
{
    private bool previousBackground;
    private int passed;

    [UnityEditor.MenuItem("Tools/Escape Stage/Run Validation")]
    public static void Run()
    {
        if (!Application.isPlaying) throw new System.InvalidOperationException("Enter Play Mode first.");
        if (FindAnyObjectByType<EscapeStageValidation>()) return;
        var runner = new GameObject("Escape Validation").AddComponent<EscapeStageValidation>();
        runner.previousBackground = Application.runInBackground;
        Application.runInBackground = true;
        DontDestroyOnLoad(runner);
    }

    private void Check(bool condition, string message)
    {
        if (!condition)
        {
            File.WriteAllText("Docs/AI/EscapeValidation.txt", "FAIL: " + message);
            throw new System.Exception(message);
        }
        passed++; Debug.Log("ESCAPE PASS: " + message);
    }

    private static void Place(EscapeStageController stage, float x)
    {
        stage.player.enabled = false;
        var rb = stage.player.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
        rb.position = new Vector2(x, -3.6f);
        Physics2D.SyncTransforms();
    }

    private IEnumerator WaitScene(string name)
    {
        float end = Time.realtimeSinceStartup+10;
        while (SceneManager.GetActiveScene().name != name && Time.realtimeSinceStartup < end) yield return null;
        Check(SceneManager.GetActiveScene().name == name, "Loaded " + name);
        yield return null;
    }

    private IEnumerator Start()
    {
        File.WriteAllText("Docs/AI/EscapeValidation.txt", "RUNNING");
        var stage = FindAnyObjectByType<EscapeStageController>();
        yield return new WaitForSeconds(0.25f);
        Check(stage && stage.houses.Length==11 && stage.playerHealth.maxHealth==1 && stage.boss.cameraShake,
            "Scene references, one-hit health and camera shake");
        Place(stage, 35);
        yield return new WaitForSeconds(0.1f);
        Check(stage.CheckHidden() && stage.IsHidden, "Grounded player fully hidden behind hut");
        Place(stage, 31.5f);
        Check(stage.CheckHidden(), "Player at visible edge of hut is also concealed");
        var rb=stage.player.GetComponent<Rigidbody2D>();
        rb.position += Vector2.up*4; Physics2D.SyncTransforms();
        Check(!stage.CheckHidden(), "Jump above roof exposes player");
        Place(stage, 35);
        Check(stage.boss.State == "Waiting" && !stage.boss.bossRenderer.enabled, "Boss waits below platform instead of following");
        float waitLimit = Time.realtimeSinceStartup + 6;
        while (stage.boss.State == "Waiting" && Time.realtimeSinceStartup < waitLimit) yield return null;
        Check(stage.boss.State == "Warning", "Periodic warning begins after wait");
        float fixedBossX = stage.boss.transform.position.x;
        Place(stage, 34);
        yield return new WaitForSeconds(0.2f);
        Check(stage.boss.IsWarning && Mathf.Abs(stage.boss.transform.position.x-fixedBossX)<0.01f,
            "One-second warning keeps boss fixed while player moves");
        waitLimit = Time.realtimeSinceStartup + 3;
        while (stage.boss.State == "Warning" && Time.realtimeSinceStartup < waitLimit) yield return null;
        Check(stage.boss.State == "Rising", "Boss begins half-second rise");
        float initialY = stage.boss.transform.position.y;
        yield return new WaitForSeconds(0.2f);
        Check(stage.boss.State == "Rising" && stage.boss.transform.position.y > initialY &&
            Mathf.Abs(stage.boss.transform.position.x-fixedBossX)<0.01f, "Boss rises vertically without chasing");
        waitLimit = Time.realtimeSinceStartup + 2;
        while (!stage.boss.IsExposed && Time.realtimeSinceStartup < waitLimit) yield return null;
        Check(stage.boss.IsExposed && !stage.playerHealth.IsDead && !stage.boss.armRenderer.enabled,
            "Cover survives reveal without drawing fist or target marker");
        yield return new WaitForSeconds(0.16f);
        Check(!stage.boss.armRenderer.enabled && !stage.boss.warning.enabled && !stage.playerHealth.IsDead,
            "Boss does not attack a player behind cover");
        yield return new WaitForSeconds(0.2f);
        Check(Mathf.Abs(stage.boss.transform.position.x-fixedBossX)>0.35f && !stage.playerHealth.IsDead,
            "Exposed boss scans nearby area while hidden player stays safe");
        Place(stage, 31.5f);
        yield return new WaitForSeconds(0.1f);
        Check(stage.CheckHidden() && !stage.boss.armRenderer.enabled && !stage.playerHealth.IsDead,
            "Player near visible hut edge stays untargeted during scan");
        waitLimit = Time.realtimeSinceStartup + 3;
        while (stage.boss.State == "Exposed" && Time.realtimeSinceStartup < waitLimit) yield return null;
        Check(stage.boss.State == "Falling", "Boss descends after exposed interval");
        waitLimit = Time.realtimeSinceStartup + 2;
        while (stage.boss.State == "Falling" && Time.realtimeSinceStartup < waitLimit) yield return null;
        Check(stage.boss.State == "Waiting" && !stage.boss.bossRenderer.enabled, "Boss hides and starts next cycle");
        // Remain outside for the next reveal: warning/rise must not kill early.
        Place(stage, 30);
        waitLimit = Time.realtimeSinceStartup + 7;
        while (stage.boss.State != "Rising" && Time.realtimeSinceStartup < waitLimit) yield return null;
        Check(stage.boss.State == "Rising" && !stage.playerHealth.IsDead, "Outside player remains alive through warning");
        waitLimit = Time.realtimeSinceStartup + 2;
        while (!stage.playerHealth.IsDead && Time.realtimeSinceStartup < waitLimit) yield return null;
        Check(stage.playerHealth.IsDead && stage.boss.IsExposed && stage.boss.armRenderer.enabled,
            "Outside player dies at reveal with fist attack");
        yield return new WaitForSecondsRealtime(0.2f);
        Check(!stage.boss.warning.enabled, "Fist finishes quickly despite defeat pause");
        Check(ScoreManager.Instance.LastResult != null && !ScoreManager.Instance.LastResult.Victory, "Reveal causes defeat");
        yield return WaitScene("전투 끝");
        FindAnyObjectByType<GameOverView>().Retry();
        yield return WaitScene("전투2");
        stage=FindAnyObjectByType<EscapeStageController>();
        Check(!stage.IsBoarding && !stage.result.IsEnding && Time.timeScale==1 && ScoreManager.Instance.LastResult==null, "Retry resets escape and pause state");
        Place(stage, 224);
        yield return new WaitForSeconds(0.2f);
        Check(!stage.result.IsEnding && !stage.IsBoarding, "Passing boss alone does not win");
        // Traverse the final ground and physically enter the ship entrance.
        rb=stage.player.GetComponent<Rigidbody2D>();
        rb.linearVelocity=new Vector2(8.75f,0);
        float deadline=Time.realtimeSinceStartup+8;
        while (!stage.IsBoarding && Time.realtimeSinceStartup<deadline)
        {
            rb.linearVelocity=new Vector2(8.75f,rb.linearVelocity.y);
            yield return new WaitForFixedUpdate();
        }
        Check(stage.IsBoarding && !rb.simulated, "Walking to entrance boards ship and disables movement");
        yield return WaitScene("전투 끝");
        Check(ScoreManager.Instance.LastResult.Victory && ScoreManager.Instance.LastResult.ObjectiveVictory, "Takeoff saves objective victory");
        Check(FindAnyObjectByType<GameOverView>().title.text == "우주선 탈출 성공!", "Result screen identifies escape victory");
        File.WriteAllText("Docs/AI/EscapeValidation.txt", "PASS: " + passed + " checks. Hut artwork cover, no attack behind cover, boss scan, timed warning/rise/fall, lethal exposed strike, retry, boarding and victory.");
        Application.runInBackground=previousBackground;
        Destroy(gameObject);
    }

    private void OnDestroy() { Application.runInBackground=previousBackground; }
}
#endif
