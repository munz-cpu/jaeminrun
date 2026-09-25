using UnityEngine;

public class Battle4Pattern4ContactDamage : MonoBehaviour
{
    private Collider2D hitbox;
    private Collider2D playerCollider;
    private EntityHealth playerHealth;
    private float damage;
    private bool hasHitPlayer;

    public void Initialize(Transform player, float contactDamage)
    {
        playerHealth = player != null ? player.GetComponent<EntityHealth>() : null;
        playerCollider = player != null ? player.GetComponent<Collider2D>() : null;
        damage = Mathf.Max(0f, contactDamage);

        hitbox = GetComponent<Collider2D>();
        if (hitbox == null)
        {
            SpriteRenderer sprite = GetComponent<SpriteRenderer>();
            if (sprite != null && sprite.sprite != null)
            {
                BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
                box.size = sprite.sprite.bounds.size;
                box.isTrigger = true;
                hitbox = box;
            }
        }
    }

    private void Update()
    {
        if (hasHitPlayer || damage <= 0f || playerHealth == null || playerHealth.IsDead ||
            hitbox == null || playerCollider == null) return;

        Physics2D.SyncTransforms();
        if (!ColliderOverlap2D.IsOverlapping(hitbox, playerCollider)) return;

        hasHitPlayer = true;
        playerHealth.TakeDamage(damage);
    }
}
