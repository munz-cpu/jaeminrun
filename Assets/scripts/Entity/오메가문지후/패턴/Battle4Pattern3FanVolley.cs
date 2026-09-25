using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle4Pattern3FanVolley : Battle4Pattern
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform firstMuzzle;
    [SerializeField] private Transform secondMuzzle;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Fan")]
    [SerializeField, Min(1)] private int projectilesPerFan = 5;
    [SerializeField, Range(0f, 180f)] private float fanAngle = 100f;
    [SerializeField] private float firstMuzzleStartAngle = -50f;
    [SerializeField] private float secondMuzzleStartAngle = -50f;
    [SerializeField, Min(1)] private int burstCount = 3;
    [SerializeField, Min(0f)] private float delayBetweenBursts = 0.6f;
    [SerializeField, Min(0f)] private float preparationDuration = 0.2f;
    [SerializeField, Min(0f)] private float delayBetweenProjectiles = 0.08f;
    [SerializeField, Range(0f, 45f)] private float angleShiftPerBurst = 12f;

    [Header("Projectile")]
    [SerializeField, Min(0.01f)] private float projectileSpeed = 12f;
    [SerializeField, Min(0.01f)] private float projectileLifetime = 2f;
    [SerializeField, Min(0f)] private float projectileDamage = 1f;

    [Header("Muzzle Color Flash")]
    [SerializeField, Min(0.01f)] private float colorFlashInterval = 0.08f;

    [Header("Sound")]
    [SerializeField] private AudioClip preparationSfx;
    [SerializeField] private AudioClip fireSfx;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private SpriteRenderer firstMuzzleRenderer;
    private SpriteRenderer secondMuzzleRenderer;
    private Color firstOriginalColor;
    private Color secondOriginalColor;
    private Coroutine colorFlashRoutine;

    public override IEnumerator Execute()
    {
        if (player == null || firstMuzzle == null || secondMuzzle == null || projectilePrefab == null)
        {
            Debug.LogError("Battle4Pattern3FanVolley needs a player, two muzzles, and a projectile prefab.", this);
            yield break;
        }

        StartColorFlash();
        int repeats = Mathf.Max(1, burstCount);
        for (int burst = 0; burst < repeats; burst++)
        {
            if (!isActiveAndEnabled || player == null)
            {
                StopColorFlash();
                yield break;
            }

            PlayPatternSound(preparationSfx);
            if (preparationDuration > 0f)
                yield return new WaitForSeconds(preparationDuration);
            if (!isActiveAndEnabled || player == null)
            {
                StopColorFlash();
                yield break;
            }

            float directionShift = (burst - (repeats - 1) * 0.5f) * angleShiftPerBurst;
            Vector2 firstCenter = DirectionToPlayer(firstMuzzle);
            Vector2 secondCenter = DirectionToPlayer(secondMuzzle);
            int count = Mathf.Max(1, projectilesPerFan);
            PlayPatternSound(fireSfx);
            for (int i = 0; i < count; i++)
            {
                if (!isActiveAndEnabled || player == null)
                {
                    StopColorFlash();
                    yield break;
                }

                float angleStep = count == 1 ? 0f : fanAngle * i / (count - 1);
                float reverseAngleStep = count == 1 ? 0f : fanAngle * (count - 1 - i) / (count - 1);
                FireProjectile(firstMuzzle, firstCenter,
                    firstMuzzleStartAngle + reverseAngleStep + directionShift);
                if (delayBetweenProjectiles > 0f)
                    yield return new WaitForSeconds(delayBetweenProjectiles);

                if (!isActiveAndEnabled || player == null)
                {
                    StopColorFlash();
                    yield break;
                }
                FireProjectile(secondMuzzle, secondCenter,
                    secondMuzzleStartAngle + angleStep + directionShift);
                if (delayBetweenProjectiles > 0f && i < count - 1)
                    yield return new WaitForSeconds(delayBetweenProjectiles);
            }
            spawned.RemoveAll(shot => shot == null);

            if (burst < repeats - 1 && delayBetweenBursts > 0f)
                yield return new WaitForSeconds(delayBetweenBursts);
        }
        StopColorFlash();
    }

    private void StartColorFlash()
    {
        StopColorFlash();
        firstMuzzleRenderer = firstMuzzle.GetComponentInChildren<SpriteRenderer>();
        secondMuzzleRenderer = secondMuzzle.GetComponentInChildren<SpriteRenderer>();
        if (firstMuzzleRenderer != null) firstOriginalColor = firstMuzzleRenderer.color;
        if (secondMuzzleRenderer != null) secondOriginalColor = secondMuzzleRenderer.color;
        colorFlashRoutine = StartCoroutine(FlashMuzzleColors());
    }

    private IEnumerator FlashMuzzleColors()
    {
        while (true)
        {
            if (firstMuzzleRenderer != null)
            {
                Color color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
                color.a = firstOriginalColor.a;
                firstMuzzleRenderer.color = color;
            }
            if (secondMuzzleRenderer != null)
            {
                Color color = Random.ColorHSV(0f, 1f, 0.8f, 1f, 0.8f, 1f);
                color.a = secondOriginalColor.a;
                secondMuzzleRenderer.color = color;
            }
            yield return new WaitForSeconds(colorFlashInterval);
        }
    }

    private void StopColorFlash()
    {
        if (colorFlashRoutine != null)
        {
            StopCoroutine(colorFlashRoutine);
            colorFlashRoutine = null;
        }
        if (firstMuzzleRenderer != null) firstMuzzleRenderer.color = firstOriginalColor;
        if (secondMuzzleRenderer != null) secondMuzzleRenderer.color = secondOriginalColor;
        firstMuzzleRenderer = null;
        secondMuzzleRenderer = null;
    }

    private Vector2 DirectionToPlayer(Transform muzzle)
    {
        Vector2 centerDirection = (player.position - muzzle.position).normalized;
        return centerDirection == Vector2.zero ? Vector2.down : centerDirection;
    }

    private void FireProjectile(Transform muzzle, Vector2 centerDirection, float angle)
    {
        Vector2 direction = Quaternion.Euler(0f, 0f, angle) * centerDirection;
        GameObject shot = Instantiate(projectilePrefab, muzzle.position,
            Quaternion.FromToRotation(Vector3.up, direction));
        Battle4AimedProjectile movement = shot.GetComponent<Battle4AimedProjectile>();
        if (movement == null) movement = shot.AddComponent<Battle4AimedProjectile>();
        movement.Launch(direction, projectileSpeed, projectileLifetime, 0f,
            player, projectileDamage, false);
        spawned.Add(shot);
    }

    public override void Cancel()
    {
        StopColorFlash();
        foreach (GameObject shot in spawned)
            if (shot != null) Destroy(shot);
        spawned.Clear();
    }

    private void OnDisable()
    {
        Cancel();
    }
}
