using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battle4Pattern4 : Battle4Pattern
{
    [Header("Prefabs")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject lowerPrefab;
    [SerializeField] private GameObject upperPrefab;

    [Header("Contact Damage")]
    [SerializeField, Min(0f)] private float lowerContactDamage = 1f;
    [SerializeField, Min(0f)] private float upperContactDamage = 5f;

    [Header("Lower Runners")]
    [SerializeField] private Vector2 lowerSpawnPosition = new Vector2(-8.5f, -4.1f);
    [SerializeField] private float lowerExitX = 9.5f;
    [SerializeField, Min(0.01f)] private float lowerMoveSpeed = 3.5f;
    [SerializeField, Min(0.01f)] private float lowerSpawnInterval = 0.65f;

    [Header("Upper Arc")]
    [SerializeField] private float upperLeftX = -7.5f;
    [SerializeField] private float upperRightX = 7.5f;
    [SerializeField] private float upperStartY = -3.3f;
    [SerializeField, Min(0.1f)] private float upperArcRadius = 5.5f;
    [SerializeField, Min(0f)] private float upperPreparationTime = 0.5f;
    [SerializeField, Min(0.01f)] private float upperArcDuration = 1.2f;
    [SerializeField, Min(0f)] private float upperRotationDegrees = 90f;
    [SerializeField, Min(0f)] private float upperEndHoldTime = 0.3f;

    private readonly List<GameObject> spawned = new List<GameObject>();
    private Coroutine lowerSpawnRoutine;
    private bool running;

    public override IEnumerator Execute()
    {
        if (player == null || player.GetComponent<EntityHealth>() == null ||
            player.GetComponent<Collider2D>() == null || lowerPrefab == null || upperPrefab == null)
        {
            Debug.LogError("Battle4Pattern4 needs a player with EntityHealth and Collider2D, plus both prefabs.", this);
            yield break;
        }

        running = true;
        lowerSpawnRoutine = StartCoroutine(SpawnLowerRunners());
        for (int appearance = 0; appearance < 4; appearance++)
        {
            if (!running || !isActiveAndEnabled) yield break;
            yield return MoveUpper(appearance % 2 == 0);
        }

        Cancel();
    }

    private IEnumerator SpawnLowerRunners()
    {
        while (running)
        {
            GameObject carrier = new GameObject("Pattern4 Lower Runner");
            carrier.transform.position = lowerSpawnPosition;
            // BouncyRun2D animates the prefab's local position; move its parent instead.
            GameObject visual = Instantiate(lowerPrefab, Vector3.zero, Quaternion.identity);
            visual.transform.SetParent(carrier.transform, false);
            Battle4Pattern4ContactDamage contact = visual.AddComponent<Battle4Pattern4ContactDamage>();
            contact.Initialize(player, lowerContactDamage);
            spawned.Add(carrier);
            StartCoroutine(MoveLower(carrier));
            spawned.RemoveAll(item => item == null);
            yield return new WaitForSeconds(lowerSpawnInterval);
        }
    }

    private IEnumerator MoveLower(GameObject carrier)
    {
        while (carrier != null && carrier.transform.position.x < lowerExitX)
        {
            carrier.transform.position += Vector3.right * (lowerMoveSpeed * Time.deltaTime);
            yield return null;
        }
        if (carrier != null) Destroy(carrier);
    }

    private IEnumerator MoveUpper(bool fromLeft)
    {
        float edgeX = fromLeft ? upperLeftX : upperRightX;
        Vector3 start = new Vector3(edgeX, upperStartY, 0f);
        GameObject upper = Instantiate(upperPrefab, start, upperPrefab.transform.rotation);
        Battle4Pattern4ContactDamage contact = upper.AddComponent<Battle4Pattern4ContactDamage>();
        contact.Initialize(player, upperContactDamage);
        spawned.Add(upper);

        if (upperPreparationTime > 0f)
            yield return new WaitForSeconds(upperPreparationTime);

        Quaternion initialRotation = upper.transform.rotation;
        float elapsed = 0f;
        while (elapsed < upperArcDuration && upper != null && running)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / upperArcDuration);
            upper.transform.position = ArcPosition(edgeX, fromLeft, progress);
            float turn = (fromLeft ? 1f : -1f) * upperRotationDegrees * progress;
            upper.transform.rotation = initialRotation * Quaternion.Euler(0f, 0f, turn);
            yield return null;
        }

        if (upper != null)
        {
            upper.transform.position = ArcPosition(edgeX, fromLeft, 1f);
            if (upperEndHoldTime > 0f)
                yield return new WaitForSeconds(upperEndHoldTime);
            Destroy(upper);
        }
    }

    private Vector3 ArcPosition(float edgeX, bool fromLeft, float progress)
    {
        float degrees = -90f + (fromLeft ? 90f : -90f) * progress;
        float radians = degrees * Mathf.Deg2Rad;
        return new Vector3(edgeX + upperArcRadius * Mathf.Cos(radians),
            upperStartY + upperArcRadius + upperArcRadius * Mathf.Sin(radians), 0f);
    }

    public override void Cancel()
    {
        running = false;
        if (lowerSpawnRoutine != null)
        {
            StopCoroutine(lowerSpawnRoutine);
            lowerSpawnRoutine = null;
        }
        StopAllCoroutines();
        foreach (GameObject item in spawned)
            if (item != null) Destroy(item);
        spawned.Clear();
    }

    private void OnDisable()
    {
        Cancel();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int side = 0; side < 2; side++)
        {
            bool fromLeft = side == 0;
            float edgeX = fromLeft ? upperLeftX : upperRightX;
            Vector3 previous = ArcPosition(edgeX, fromLeft, 0f);
            for (int segment = 1; segment <= 16; segment++)
            {
                Vector3 next = ArcPosition(edgeX, fromLeft, segment / 16f);
                Gizmos.DrawLine(previous, next);
                previous = next;
            }
        }
    }
}
