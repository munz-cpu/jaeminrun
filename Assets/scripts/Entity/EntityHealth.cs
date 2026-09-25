using System.Collections;
using UnityEngine;
public class EntityHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    [SerializeField] bool destroyThis = true;
    [SerializeField] public float currentHealth;
    [SerializeField] private Sprite originalSprite;
    [SerializeField] private Sprite hitSprite;
    [SerializeField] private float hitEffectSec = 0.2f;
    [SerializeField, Min(0f)] private float invincibilityDuration = 0f;
    [SerializeField] bool turnRed = false;
    [SerializeField] private Color hitColor = Color.red;

    [SerializeField] private AudioClip hitSfx;


    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Coroutine hitRoutine;
    private float nextDamageTime;
    public float CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0f;
    public event System.Action<float> Damaged;
    public event System.Action Died;
    AudioSource ads;
    private void Awake()
    {
        ads = GetComponent<AudioSource>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            if (originalSprite == null) originalSprite = spriteRenderer.sprite;
            originalColor = spriteRenderer.color;
        }
        currentHealth = maxHealth;
    }
    public void TakeDamage(float damage)
    {
        if (IsDead || damage <= 0f || Time.time < nextDamageTime) return;
        float appliedDamage = Mathf.Min(currentHealth, damage);
        currentHealth = Mathf.Max(0f, currentHealth-damage);
        nextDamageTime = Time.time + invincibilityDuration;
        Damaged?.Invoke(appliedDamage);
        if (IsDead)
        {
            Died?.Invoke();
            if (destroyThis) gameObject.SetActive(false);
            if (destroyThis) Destroy(gameObject);
            return;
        }
        if (spriteRenderer == null) return;
        if (hitRoutine != null) StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(HitEffect());
    }
    private IEnumerator HitEffect()
    {
        if (hitSfx) ads.PlayOneShot(hitSfx);
        if (hitSprite != null) spriteRenderer.sprite = hitSprite;
        if (turnRed) spriteRenderer.color = new Color(hitColor.r, hitColor.g, hitColor.b, originalColor.a);
        yield return new WaitForSeconds(hitEffectSec);
        spriteRenderer.sprite = originalSprite;
        spriteRenderer.color = originalColor;
        hitRoutine = null;
    }

    private void OnDisable()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.sprite = originalSprite;
        spriteRenderer.color = originalColor;
    }
}
