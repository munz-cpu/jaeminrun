using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossPatern1 : MonoBehaviour
{
    [Header("References")]
    public RunnerMovement player;
    public Camera arenaCamera;
    public BossProjectile cookiePrefab;
    public BossMinion minionPrefab;
    public Transform mouth;
    public Vector3 mouthOffset = new Vector3(-0.2f, 1.2f, 0f);
    [Header("Timing")]
    [Min(0f)] public float initialDelay = 2f;
    [Min(0.1f)] public float patternInterval = 2f;
    [Min(1)] public int shotCount = 5;
    [Min(0.1f)] public float shotInterval = 0.65f;
    [Min(0.1f)] public float flightTime = 1.3f;
    [Min(0.1f)] public float projectileGravity = 18f;
    [Header("Swing")]
    [Min(0.1f)] public float exitDuration = 0.7f;
    [Min(0f)] public float hiddenDelay = 0.8f;
    [Min(0.1f)] public float warningDuration = 1f;
    [Min(0.1f)] public float swingDuration = 1.5f;
    public float swingBottomY = 8f;
    [Min(0.1f)] public float offscreenMargin = 4f;
    [Min(0f)] public float swingDamage = 1f;
    [Min(0f)] public float swingDamageInterval = 1f;
    [Header("Minions")]
    [Min(1)] public int minionCount = 3;
    [Min(0.1f)] public float minionInterval = 0.8f;
    public float minionY = -1.6f;
    [Header("Combined Patterns")]
    [Range(0f, 1f)] public float combinedPatternChance = 0.25f;
    private Rigidbody2D body;
    private EntityHealth health;
    private FloatingObject floating;
    private Collider2D hitbox;
    private EntityHealth playerHealth;
    private Collider2D playerCollider;
    private Vector3 home;
    private GameObject warning;
    private float nextSwingDamageTime;
    private readonly List<GameObject> spawned = new List<GameObject>();
    public string CurrentPattern { get; private set; } = "Idle";
    public bool MinionsSpawning { get; private set; }
    public Vector3 MouthPosition => mouth != null ? mouth.position : transform.TransformPoint(mouthOffset);

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<EntityHealth>();
        floating = GetComponent<FloatingObject>();
        hitbox = GetComponent<Collider2D>();
        home = transform.position;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }

    private void OnEnable()
    {
        if (floating != null)
        {
            floating.SetAnchor(home);
            floating.SetFloating(true);
        }
        StartCoroutine(Patterns());
    }
    private IEnumerator Patterns()
    {
        yield return new WaitForSeconds(initialDelay);
        if (player == null || arenaCamera == null || cookiePrefab == null || minionPrefab == null)
        {
            Debug.LogError("Boss pattern references are missing.", this);
            yield break;
        }
        playerHealth = player.GetComponent<EntityHealth>();
        playerCollider = player.GetComponent<Collider2D>();
        while (health == null || !health.IsDead)
        {
            bool combine = Random.value < combinedPatternChance;
            bool combineWithCookies = combine && Random.value < 0.5f;
            Coroutine combinedMinions = null;

            CurrentPattern = "Cookies";
            if (combineWithCookies)
                combinedMinions = StartCoroutine(SpawnMinions());
            for (int i = 0; i < shotCount; i++)
            {
                if (player == null) yield break;
                var cookie = Instantiate(cookiePrefab, MouthPosition, Quaternion.identity);
                cookie.Launch(player, flightTime, projectileGravity);
                spawned.Add(cookie.gameObject);
                yield return new WaitForSeconds(shotInterval);
            }
            CurrentPattern = "Idle";
            if (combinedMinions != null)
                yield return combinedMinions;
            yield return new WaitForSeconds(patternInterval);
            CurrentPattern = "Swing";
            if (combine && !combineWithCookies)
                combinedMinions = StartCoroutine(SpawnMinions());
            // FloatingObject and Rigidbody2D must not write the boss position together.
            // Keep the component enabled, but pause only its positional animation.
            if (floating != null) floating.SetFloating(false);
            body.position = transform.position;
            var visualBounds = GetComponent<SpriteRenderer>().bounds;
            float top = arenaCamera.transform.position.y + arenaCamera.orthographicSize
                + Mathf.Max(offscreenMargin, transform.position.y-visualBounds.min.y+offscreenMargin);
            yield return MoveTo(new Vector3(home.x, top, home.z), exitDuration);
            yield return new WaitForSeconds(hiddenDelay);
            for (int pass = 0; pass < 2; pass++)
            {
                float halfWidth = arenaCamera.orthographicSize * arenaCamera.aspect + offscreenMargin + visualBounds.extents.x;
                float centerX = arenaCamera.transform.position.x;
                Vector3 left = new Vector3(centerX-halfWidth, top, home.z);
                Vector3 right = new Vector3(centerX+halfWidth, top, home.z);
                Vector3 start = pass == 0 ? left : right;
                Vector3 end = pass == 0 ? right : left;
                body.position = start;
                ShowWarning(new Vector3(centerX, swingBottomY, home.z));
                yield return new WaitForSeconds(warningDuration);
                Destroy(warning);
                // The quadratic curve reaches swingBottomY at its midpoint.
                Vector3 control = new Vector3(centerX, 2f*swingBottomY-top, home.z);
                float elapsed = 0f;
                while (elapsed < swingDuration)
                {
                    elapsed += Time.fixedDeltaTime;
                    float t = Mathf.Clamp01(elapsed / swingDuration);
                    body.MovePosition((1f-t)*(1f-t)*start + 2f*(1f-t)*t*control + t*t*end);
                    yield return new WaitForFixedUpdate();
                }
            }
            yield return MoveTo(new Vector3(home.x, top, home.z), exitDuration);
            yield return MoveTo(home, exitDuration);
            if (floating != null)
            {
                floating.SetAnchor(home);
                floating.SetFloating(true);
            }
            CurrentPattern = "Idle";
            if (combine && !combineWithCookies)
                yield return combinedMinions;
            yield return new WaitForSeconds(patternInterval);
            if (!combine)
            {
                CurrentPattern = "Minions";
                yield return SpawnMinions();
                yield return new WaitForSeconds(patternInterval);
            }
        }
    }

    private IEnumerator SpawnMinions()
    {
        MinionsSpawning = true;
        for (int i = 0; i < minionCount; i++)
        {
            if (player == null)
                break;

            float halfWidth = arenaCamera.orthographicSize * arenaCamera.aspect + offscreenMargin;
            var minion = Instantiate(minionPrefab,
                new Vector3(arenaCamera.transform.position.x-halfWidth, minionY, 0f), Quaternion.identity);
            minion.SetExitX(arenaCamera.transform.position.x+halfWidth);
            minion.SetTarget(player);
            spawned.Add(minion.gameObject);
            yield return new WaitForSeconds(minionInterval);
        }
        spawned.RemoveAll(item => item == null);
        MinionsSpawning = false;
    }
    private IEnumerator MoveTo(Vector3 end, float duration)
    {
        Vector3 start = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            body.MovePosition(Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, elapsed/duration)));
            yield return new WaitForFixedUpdate();
        }
    }
    private void ShowWarning(Vector3 position)
    {
        warning = new GameObject("Swing Warning");
        warning.transform.position = position;
        var text = warning.AddComponent<TMPro.TextMeshPro>();
        text.text = "!";
        text.fontSize = 12f;
        text.color = Color.red;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.sortingOrder = 100;
    }

    private void FixedUpdate()
    {
        if (CurrentPattern != "Swing" || Time.time < nextSwingDamageTime)
            return;

        if (playerHealth == null || playerHealth.IsDead)
            return;

        if (!ColliderOverlap2D.IsOverlapping(hitbox, playerCollider))
            return;

        playerHealth.TakeDamage(swingDamage);
        nextSwingDamageTime = Time.time + swingDamageInterval;
    }
    private void OnDisable()
    {
        StopAllCoroutines();
        if (warning != null) Destroy(warning);
        foreach (var item in spawned) if (item != null) Destroy(item);
        spawned.Clear();
        if (body != null) body.position = home;
        if (floating != null)
        {
            floating.SetAnchor(home);
            floating.SetFloating(true);
        }
        MinionsSpawning = false;
        CurrentPattern = "Idle";
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(MouthPosition, 0.15f);
        Gizmos.DrawLine(transform.position, MouthPosition);
    }
}
