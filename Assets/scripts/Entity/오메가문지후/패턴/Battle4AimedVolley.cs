using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle4AimedVolley : Battle4Pattern
{
    [SerializeField] private string 컴포넌트이름;
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Random Fire Positions (Local)")]
    [SerializeField, Min(1)] private int projectileCount = 3;
    [SerializeField] private Vector2 fireAreaMin = new Vector2(-2f, -1f);
    [SerializeField] private Vector2 fireAreaMax = new Vector2(2f, 1f);

    [Header("Side Aiming")]
    [SerializeField, Min(0)] private int sideAimCount = 2;
    [SerializeField, Min(0f)] private float sideAimDistanceMin = 0.6f;
    [SerializeField, Min(0f)] private float sideAimDistanceMax = 1.3f;

    [Header("Timing")]
    [SerializeField, Min(0f)] private float delayBetweenShots = 0.25f;
    [SerializeField, Min(1)] private int flashCount = 4;
    [SerializeField, Min(0.01f)] private float flashInterval = 0.1f;
    [SerializeField, Min(0.01f)] private float projectileLifetime = 2f;
    [SerializeField, Min(0f)] private float wallHoldTime = 0.75f;
    [SerializeField, Min(0.01f)] private float projectileSpeed = 12f;

    [Header("Warning Lines")]
    [SerializeField, Min(0.01f)] private float lineWidth = 0.07f;
    [SerializeField, Min(1f)] private float lineLengthPastPlayer = 30f;
    [SerializeField] private int sortingOrder = 100;

    [Header("Sound")]
    [SerializeField] private AudioClip preparationSfx;
    [SerializeField] private AudioClip fireSfx;

    private readonly List<LineRenderer> lines = new List<LineRenderer>();
    private readonly List<GameObject> projectiles = new List<GameObject>();
    private Material lineMaterial;
    private int activeShots;

    public override IEnumerator Execute()
    {
        if (player == null || projectilePrefab == null)
        {
            Debug.LogError("Battle4AimedVolley needs a player and projectile prefab.", this);
            yield break;
        }

        Vector3[] firePositions = ChooseFirePositions();
        bool[] sideAims = ChooseSideAims(firePositions.Length);
        activeShots = 0;
        PlayPatternSound(preparationSfx);
        for (int shotIndex = 0; shotIndex < firePositions.Length; shotIndex++)
        {
            if (!isActiveAndEnabled) yield break;
            activeShots++;
            StartCoroutine(PrepareAndFire(firePositions[shotIndex], sideAims[shotIndex],
                shotIndex == 0));
            if (delayBetweenShots > 0f && shotIndex < firePositions.Length - 1)
                yield return new WaitForSeconds(delayBetweenShots);
        }

        while (activeShots > 0 && isActiveAndEnabled)
            yield return null;
    }

    private IEnumerator PrepareAndFire(Vector3 firePosition, bool aimAside, bool playFireSound)
    {
        if (player == null)
        {
            activeShots--;
            yield break;
        }

        Vector3 target = player.position;
        target.z = 0f;
        if (aimAside)
        {
            float distance = Random.Range(Mathf.Min(sideAimDistanceMin, sideAimDistanceMax),
                Mathf.Max(sideAimDistanceMin, sideAimDistanceMax));
            target.x += Random.value < 0.5f ? -distance : distance;
        }
        LineRenderer line = CreateLine(firePosition, target);
        for (int i = 0; i < flashCount * 2; i++)
        {
            Color color = i % 2 == 0 ? Color.yellow : Color.red;
            if (line != null) line.startColor = line.endColor = color;
            yield return new WaitForSeconds(flashInterval);
        }

        RemoveLine(line);
        if (playFireSound) PlayPatternSound(fireSfx);
        Vector2 direction = (target - firePosition).normalized;
        GameObject shot = Instantiate(projectilePrefab, firePosition,
            Quaternion.FromToRotation(Vector3.up, direction));
        Battle4AimedProjectile movement = shot.GetComponent<Battle4AimedProjectile>();
        if (movement == null) movement = shot.AddComponent<Battle4AimedProjectile>();
        movement.Launch(direction, projectileSpeed, projectileLifetime, wallHoldTime, player);
        projectiles.Add(shot);
        projectiles.RemoveAll(item => item == null);
        activeShots--;
    }

    private Vector3[] ChooseFirePositions()
    {
        Vector3[] positions = new Vector3[Mathf.Max(1, projectileCount)];
        float minX = Mathf.Min(fireAreaMin.x, fireAreaMax.x);
        float maxX = Mathf.Max(fireAreaMin.x, fireAreaMax.x);
        float minY = Mathf.Min(fireAreaMin.y, fireAreaMax.y);
        float maxY = Mathf.Max(fireAreaMin.y, fireAreaMax.y);

        for (int i = 0; i < positions.Length; i++)
        {
            Vector3 localPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
            positions[i] = transform.TransformPoint(localPosition);
        }
        return positions;
    }

    private bool[] ChooseSideAims(int count)
    {
        bool[] sideAims = new bool[count];
        int[] indices = new int[count];
        for (int i = 0; i < count; i++) indices[i] = i;
        for (int i = count - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            int current = indices[i];
            indices[i] = indices[swapIndex];
            indices[swapIndex] = current;
        }

        int sideCount = Mathf.Clamp(sideAimCount, 0, count - 1);
        for (int i = 0; i < sideCount; i++) sideAims[indices[i]] = true;
        return sideAims;
    }

    private LineRenderer CreateLine(Vector3 firePosition, Vector3 target)
    {
        if (lineMaterial == null)
            lineMaterial = new Material(Shader.Find("Sprites/Default"));

        GameObject lineObject = new GameObject("Aimed Volley Warning");
        lineObject.transform.SetParent(transform, false);
        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.material = lineMaterial;
        line.useWorldSpace = true;
        line.positionCount = 2;
        Vector3 aimDirection = (target - firePosition).normalized;
        line.SetPosition(0, firePosition);
        line.SetPosition(1, target + aimDirection * lineLengthPastPlayer);
        line.startWidth = line.endWidth = lineWidth;
        line.sortingOrder = sortingOrder;
        lines.Add(line);
        return line;
    }

    private void ClearLines()
    {
        foreach (LineRenderer line in lines)
            if (line != null) Destroy(line.gameObject);
        lines.Clear();
    }

    private void RemoveLine(LineRenderer line)
    {
        lines.Remove(line);
        if (line != null) Destroy(line.gameObject);
    }

    public override void Cancel()
    {
        StopAllCoroutines();
        activeShots = 0;
        ClearLines();
        foreach (GameObject shot in projectiles)
            if (shot != null) Destroy(shot);
        projectiles.Clear();
        if (lineMaterial != null) Destroy(lineMaterial);
        lineMaterial = null;
    }

    private void OnDisable()
    {
        Cancel();
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 min = Vector2.Min(fireAreaMin, fireAreaMax);
        Vector2 max = Vector2.Max(fireAreaMin, fireAreaMax);
        Vector2 center = (min + max) * 0.5f;
        Vector2 size = max - min;

        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(new Vector3(center.x, center.y, 0f), new Vector3(size.x, size.y, 0f));
        Gizmos.matrix = Matrix4x4.identity;
    }
}
