using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Battle3GameController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Battle3FlightController player;
    [SerializeField] private Battle3Projectile projectilePrefab;

    [Header("Projectile Spawning")]
    [SerializeField] private float initialSpawnDelay = 1f;
    [SerializeField] private float spawnInterval = 1.4f;
    [SerializeField, Range(0f, 5f)] private float minimumSpawnHeight = 0.12f;
    [SerializeField, Range(0f, 5f)] private float maximumSpawnHeight = 0.88f;
    [SerializeField] private float spawnViewportX = 1.1f;
    [SerializeField] private float minimumFlightAngle = -12f;
    [SerializeField] private float maximumFlightAngle = 12f;

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

        if (gameCamera == null || player == null || projectilePrefab == null)
        {
            Debug.LogError("Battle3GameController requires a camera, player, and projectile prefab.", this);
            enabled = false;
            return;
        }

        ScoreManager.GetOrCreate().BeginBattle();
        StartCoroutine(SpawnProjectiles());
    }

    private void Update()
    {
        if (isGameOver)
            return;

        elapsedTime += Time.deltaTime;

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
            yield return new WaitForSeconds(spawnInterval);
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
        projectile.Initialize(this, direction);
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

    private IEnumerator LoadResultScene()
    {
        yield return new WaitForSeconds(gameOverDelay);

        if (Application.CanStreamedLevelBeLoaded(resultScenePath))
            yield return SceneManager.LoadSceneAsync(resultScenePath);
        else
            Debug.LogError($"Result scene is not enabled in Build Settings: {resultScenePath}", this);
    }
}
