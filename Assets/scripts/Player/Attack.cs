using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Attack : MonoBehaviour
{
    [Header("Popcorn Attack")]
    [SerializeField] private Projectile popcornPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 12f;
    [SerializeField, Min(0.1f)] private float projectileLifetime = 5f;
    [SerializeField, Min(0f)] private float attackCooldown = 0.15f;
    [SerializeField] AudioClip shootSfx; 

    [Header("Attack Text")]
    [SerializeField] private string attackText = "popcorn!";
    [SerializeField] private Color attackTextColor = new Color(1f, 0.82f, 0.15f, 1f);
    [SerializeField, Min(0.1f)] private float attackTextDuration = 0.9f;
    [SerializeField, Min(0f)] private float attackTextRiseDistance = 1.2f;
    [SerializeField, Min(0.1f)] private float attackTextFontSize = 3.5f;
    [SerializeField] private Vector2 attackTextRandomOffset = new Vector2(0.8f, 0.45f);

    [SerializeField] private TMP_FontAsset attackTextFont;

    private Camera mainCamera;
    private InputAction attackAction;
    private float nextAttackTime;

    AudioSource ads;

    private void Awake()
    {
        ads = GetComponent<AudioSource>();
        mainCamera = Camera.main;
        PlayerInput playerInput = GetComponent<PlayerInput>();
        attackAction = playerInput != null ? playerInput.actions.FindAction("Attack", false) : null;
    }

    private void Update()
    {
        if (attackAction != null && attackAction.IsPressed() && Time.time >= nextAttackTime)
        {
            ShootTowardMouse();
        }
    }

    // PlayerInput의 Attack 액션이 Send Messages 방식으로 이 함수를 호출합니다.
    public void OnAttack(InputValue value)
    {
        if (!value.isPressed || Time.time < nextAttackTime)
        {
            return;
        }

        ShootTowardMouse();
    }

    private void ShootTowardMouse()
    {
        if (popcornPrefab == null || Mouse.current == null)
        {
            return;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return;
        }
        if (shootSfx) ads.PlayOneShot(shootSfx);

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(
            new Vector3(mouseScreenPosition.x, mouseScreenPosition.y,
                Mathf.Abs(mainCamera.transform.position.z - spawnPosition.z)));

        Vector2 direction = ((Vector2)mouseWorldPosition - (Vector2)spawnPosition).normalized;
        if (direction.sqrMagnitude <= Mathf.Epsilon)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Projectile projectile = Instantiate(
            popcornPrefab,
            spawnPosition,
            Quaternion.Euler(0f, 0f, angle));

        projectile.Launch(direction, projectileSpeed, projectileLifetime, SpawnAttackText);
        nextAttackTime = Time.time + attackCooldown;
    }

    private void SpawnAttackText(Vector3 hitPosition)
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-attackTextRandomOffset.x, attackTextRandomOffset.x),
            Random.Range(-attackTextRandomOffset.y, attackTextRandomOffset.y),
            0f);

        GameObject textObject = new GameObject("Popcorn Attack Text");
        textObject.transform.position = hitPosition + randomOffset;

        TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.text = attackText;
        textMesh.fontSize = attackTextFontSize;
        textMesh.color = attackTextColor;
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.sortingOrder = 100;
        textMesh.textWrappingMode = TextWrappingModes.NoWrap;
        if (attackTextFont != null)
        {
            textMesh.font = attackTextFont;
        }

        Destroy(textObject, attackTextDuration + 0.1f);
        StartCoroutine(RiseAndFadeText(textObject.transform, textMesh));
    }

    private IEnumerator RiseAndFadeText(Transform textTransform, TextMeshPro textMesh)
    {
        Vector3 startPosition = textTransform.position;
        Vector3 endPosition = startPosition + Vector3.up * attackTextRiseDistance;
        Color startColor = attackTextColor;
        float elapsed = 0f;

        while (elapsed < attackTextDuration && textMesh != null)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / attackTextDuration);
            float easedProgress = 1f - Mathf.Pow(1f - progress, 2f);

            textTransform.position = Vector3.Lerp(startPosition, endPosition, easedProgress);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, 1f - progress);
            yield return null;
        }

        if (textTransform != null)
        {
            Destroy(textTransform.gameObject);
        }
    }
}
