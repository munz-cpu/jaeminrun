using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Battle3GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Battle3FlightController player;
    [SerializeField] private Battle3Projectile projectilePrefab;
    [SerializeField] private Transform earth;

    [Header("Projectile Spawning")]
    [SerializeField] private float initialSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 1.4f;
    [SerializeField, Min(0.1f)] private float minimumSpawnInterval = 0.75f;
    [SerializeField, Min(1f)] private float maximumProjectileSpeedMultiplier = 1.6f;
    [SerializeField, Range(0f, 5f)] private float minimumSpawnHeight = 0.12f;
    [SerializeField, Range(0f, 5f)] private float maximumSpawnHeight = 0.88f;
    [SerializeField] private float spawnViewportX = 1.1f;
    [SerializeField] private float minimumFlightAngle = -12f;
    [SerializeField] private float maximumFlightAngle = 12f;

    [Header("Clear Sequence")]
    [SerializeField, Min(1f)] private float survivalDuration = 45f;
    [SerializeField, Min(0.1f)] private float earthApproachDuration = 2f;
    [SerializeField, Min(0.1f)] private float playerArrivalDuration = 1.5f;
    [SerializeField, Range(0f, 1f)] private float earthTargetViewportX = 0.78f;
    [SerializeField, Range(0f, 1f)] private float playerTargetViewportX = 0.58f;
    [SerializeField] Slider slider;

    [Header("Game Over")]
    [SerializeField] private string resultScenePath = "Assets/Scenes/전투 끝.unity";
    [SerializeField] private string stageDisplayName = "전투 3 비행";
    [SerializeField] private float gameOverDelay = 0.25f;

    public Camera GameCamera => gameCamera;

    private float elapsedTime;
    private bool isGameOver;

    private void Awake()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (gameCamera == null || player == null || projectilePrefab == null || earth == null)
        {
            Debug.LogError("Battle3GameController requires a camera, player, projectile prefab, and earth.", this);
            enabled = false;
            return;
        }
        slider.maxValue = survivalDuration;
        ScoreManager.GetOrCreate().BeginBattle();
        StartCoroutine(SpawnProjectiles());
    }

    private void Update()
    {
        if (isGameOver)
            return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= survivalDuration)
        {
            StartCoroutine(PlayClearSequence());
            return;
        }

        slider.value = elapsedTime;



        Vector3 viewportPosition = gameCamera.WorldToViewportPoint(player.transform.position);
        if (viewportPosition.y < 0f || viewportPosition.y > 1f)
            GameOver();
    }

    private IEnumerator SpawnProjectiles()
    {
        yield return new WaitForSeconds(initialSpawnDelay);

        while (!isGameOver)
        {
            SpawnProjectile();
            float difficulty = Mathf.Clamp01(elapsedTime / survivalDuration);
            float currentSpawnInterval = Mathf.Lerp(spawnInterval, minimumSpawnInterval, difficulty);
            yield return new WaitForSeconds(currentSpawnInterval);
        }
    }

    private void SpawnProjectile()
    {
        float viewportY = Random.Range(minimumSpawnHeight, maximumSpawnHeight);
        Vector3 spawnPosition = gameCamera.ViewportToWorldPoint(new Vector3(
            spawnViewportX,
            viewportY,
            Mathf.Abs(gameCamera.transform.position.z)
        ));
        spawnPosition.z = 0f;

        float flightAngle = Random.Range(minimumFlightAngle, maximumFlightAngle);
        Vector2 direction = Quaternion.Euler(0f, 0f, flightAngle) * Vector2.left;
        Battle3Projectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        float difficulty = Mathf.Clamp01(elapsedTime / survivalDuration);
        float speedMultiplier = Mathf.Lerp(1f, maximumProjectileSpeedMultiplier, difficulty);
        projectile.Initialize(this, direction, speedMultiplier);
    }

    public void GameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        player.enabled = false;
        player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        ScoreManager.GetOrCreate().SaveResult(new BattleResult(
            stageDisplayName,
            gameObject.scene.path,
            false,
            elapsedTime,
            0f,
            1f,
            0f,
            0
        ));

        StartCoroutine(LoadResultScene());
    }

    private IEnumerator PlayClearSequence()
    {
        isGameOver = true;

        player.enabled = false;
        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        playerBody.linearVelocity = Vector2.zero;
        playerBody.simulated = false;

        foreach (Battle3Projectile projectile in FindObjectsByType<Battle3Projectile>())
            Destroy(projectile.gameObject);

        Vector3 earthStart = earth.position;
        Vector3 earthTarget = ViewportPointAtDepth(earthTargetViewportX, 0.5f, earthStart.z);
        yield return MoveTransform(earth, earthStart, earthTarget, earthApproachDuration);

        Vector3 playerStart = player.transform.position;
        Vector3 playerTarget = ViewportPointAtDepth(playerTargetViewportX, 0.5f, playerStart.z);
        yield return MoveTransform(player.transform, playerStart, playerTarget, playerArrivalDuration);

        ScoreManager.GetOrCreate().SaveResult(new BattleResult(
            stageDisplayName,
            gameObject.scene.path,
            true,
            elapsedTime,
            1f,
            1f,
            0f,
            1000,
            true
        ));

        StartCoroutine(LoadResultScene());
    }

    private IEnumerator MoveTransform(Transform target, Vector3 start, Vector3 end, float duration)
    {
        for (float time = 0f; time < duration; time += Time.deltaTime)
        {
            float progress = Mathf.SmoothStep(0f, 1f, time / duration);
            target.position = Vector3.LerpUnclamped(start, end, progress);
            yield return null;
        }

        target.position = end;
    }

    private Vector3 ViewportPointAtDepth(float viewportX, float viewportY, float worldZ)
    {
        float depth = Mathf.Abs(worldZ - gameCamera.transform.position.z);
        Vector3 worldPoint = gameCamera.ViewportToWorldPoint(new Vector3(viewportX, viewportY, depth));
        worldPoint.z = worldZ;
        return worldPoint;
    }

    private IEnumerator LoadResultScene()
    {
        yield return new WaitForSeconds(gameOverDelay);

        if (Application.CanStreamedLevelBeLoaded(resultScenePath))
            yield return SceneManager.LoadSceneAsync(resultScenePath);
        else
            Debug.LogError($"Result scene is not enabled in Build Settings: {resultScenePath}", this);
    }
}
