using UnityEngine;
[RequireComponent(typeof(EntityHealth), typeof(Rigidbody2D))]
public class BossMinion : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float moveSpeed = 2f;
    [SerializeField, Min(1f)] private float lifetime = 30f;
    [SerializeField, Min(0f)] private float contactDamage = 1f;
    [SerializeField, Min(0f)] private float contactDamageInterval = 1f;
    public LayerMask groundLayer;
    private BoxCollider2D hitbox;
    private Rigidbody2D body;
    private EntityHealth health;
    private RunnerMovement player;
    private EntityHealth playerHealth;
    private Collider2D playerCollider;
    private float exitX = float.PositiveInfinity;
    private float nextPlayerDamageTime;
    public EntityHealth Health => health;
    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        health = GetComponent<EntityHealth>();
        hitbox = GetComponent<BoxCollider2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
    }
    private void Start() { Destroy(gameObject, lifetime); }
    public void SetExitX(float value) { exitX = value; }
    public void SetTarget(RunnerMovement target)
    {
        player = target;
        playerHealth = target != null ? target.GetComponent<EntityHealth>() : null;
        playerCollider = target != null ? target.GetComponent<Collider2D>() : null;
    }
    public void HitByPopcorn() { health.TakeDamage(1f); }
    public void HitBySlam() { health.TakeDamage(health.maxHealth); }
    private void FixedUpdate()
    {
        if (health.IsDead) return;
        Vector2 next = body.position + Vector2.right*moveSpeed*Time.fixedDeltaTime;
        if (groundLayer.value != 0 && hitbox != null)
        {
            var ground = Physics2D.Raycast(next + Vector2.up*10f, Vector2.down, 40f, groundLayer);
            if (ground.collider != null)
                next.y = ground.point.y + hitbox.size.y*0.5f - hitbox.offset.y;
        }
        body.MovePosition(next);
        CheckPlayerContact();
        if (body.position.x > exitX) Destroy(gameObject);
    }

    private void CheckPlayerContact()
    {
        if (player == null || playerHealth == null || playerHealth.IsDead)
            return;

        if (!ColliderOverlap2D.IsOverlapping(hitbox, playerCollider))
            return;

        if (player.IsSlamming)
        {
            HitBySlam();
            return;
        }

        if (Time.time < nextPlayerDamageTime)
            return;

        playerHealth.TakeDamage(contactDamage);
        nextPlayerDamageTime = Time.time + contactDamageInterval;
    }
}
