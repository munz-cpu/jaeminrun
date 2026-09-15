using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BossProjectile : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float lifetime = 6f;
    [SerializeField, Min(0f)] private float damage = 1f;
    private Collider2D hitbox;
    private EntityHealth playerHealth;
    private Collider2D playerCollider;
    private bool consumed;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
    }
    public static Vector2 AimVelocity(Vector2 start, Vector2 target, float duration, float gravity)
    {
        duration = Mathf.Max(0.1f, duration);
        return (target-start)/duration + Vector2.up*(gravity*duration*0.5f);
    }
    public void Launch(Vector2 target, float duration, float gravity)
    {
        var body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.gravityScale = gravity/Mathf.Max(0.01f, -Physics2D.gravity.y);
        body.linearVelocity = AimVelocity(body.position, target, duration, gravity);
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        Destroy(gameObject, lifetime);
    }

    public void Launch(RunnerMovement player, float duration, float gravity)
    {

        if (player != null)
        {
            playerHealth = player.GetComponent<EntityHealth>();
            playerCollider = player.GetComponent<Collider2D>();
            Launch(player.transform.position, duration, gravity);
        }
    }

    private void FixedUpdate()
    {
        if (consumed || playerHealth == null || playerHealth.IsDead)
            return;

        if (!ColliderOverlap2D.IsOverlapping(hitbox, playerCollider))
            return;

        consumed = true;
        playerHealth.TakeDamage(damage);
        Destroy(gameObject);
    }
}
