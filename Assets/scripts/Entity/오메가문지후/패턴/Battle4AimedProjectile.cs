using UnityEngine;

public class Battle4AimedProjectile : MonoBehaviour
{
    private Vector2 direction;
    private float speed;
    private float remainingFlightTime;
    private float wallHoldTime;
    private Camera arenaCamera;
    private SpriteRenderer sprite;
    private bool hasEnteredView;
    private bool stopped;
    private bool stopAtWall;
    private bool hasHitPlayer;
    private Collider2D hitbox;
    private Collider2D playerCollider;
    private EntityHealth playerHealth;
    private float playerDamage = 5f;

    public void Launch(Vector2 aimDirection, float movementSpeed, float lifetime, float holdTime,
        Transform player, float damage = 5f, bool stopAtWall = true)
    {
        direction = aimDirection.normalized;
        speed = movementSpeed;
        remainingFlightTime = lifetime;
        wallHoldTime = holdTime;
        this.stopAtWall = stopAtWall;
        playerDamage = Mathf.Max(0f, damage);
        arenaCamera = Camera.main;
        sprite = GetComponent<SpriteRenderer>();
        hitbox = GetComponent<Collider2D>();
        if (hitbox == null && sprite != null && sprite.sprite != null)
        {
            BoxCollider2D box = gameObject.AddComponent<BoxCollider2D>();
            box.size = sprite.sprite.bounds.size;
            box.isTrigger = true;
            hitbox = box;
        }
        playerCollider = player != null ? player.GetComponent<Collider2D>() : null;
        playerHealth = player != null ? player.GetComponent<EntityHealth>() : null;
        hasEnteredView = arenaCamera != null && IsInsideView(arenaCamera.WorldToViewportPoint(transform.position));
        stopped = false;
        hasHitPlayer = false;
    }

    private void Update()
    {
        CheckPlayerHit();
        if (stopped) return;

        Vector3 nextPosition = transform.position + (Vector3)(direction * (speed * Time.deltaTime));
        if (stopAtWall && arenaCamera != null)
        {
            Vector3 currentView = arenaCamera.WorldToViewportPoint(transform.position);
            Vector3 nextView = arenaCamera.WorldToViewportPoint(nextPosition);
            if (IsInsideView(nextView)) hasEnteredView = true;
            else if (hasEnteredView)
            {
                float fraction = BoundaryFraction(currentView, nextView);
                transform.position = Vector3.Lerp(transform.position, nextPosition, fraction);
                stopped = true;
                CheckPlayerHit();
                Destroy(gameObject, wallHoldTime);
                return;
            }
        }

        transform.position = nextPosition;
        CheckPlayerHit();
        remainingFlightTime -= Time.deltaTime;
        if (remainingFlightTime <= 0f) Destroy(gameObject);
    }

    private void CheckPlayerHit()
    {
        if (hasHitPlayer || playerHealth == null || playerHealth.IsDead ||
            hitbox == null || playerCollider == null) return;

        Physics2D.SyncTransforms();
        if (!ColliderOverlap2D.IsOverlapping(hitbox, playerCollider)) return;

        hasHitPlayer = true;
        playerHealth.TakeDamage(playerDamage);
    }

    private bool IsInsideView(Vector3 viewport)
    {
        Vector2 padding = ViewPadding();
        return viewport.x >= padding.x && viewport.x <= 1f - padding.x
            && viewport.y >= padding.y && viewport.y <= 1f - padding.y;
    }

    private float BoundaryFraction(Vector3 start, Vector3 end)
    {
        Vector2 padding = ViewPadding();
        float fraction = 1f;
        if (end.x < padding.x) fraction = Mathf.Min(fraction, (padding.x - start.x) / (end.x - start.x));
        if (end.x > 1f - padding.x) fraction = Mathf.Min(fraction, (1f - padding.x - start.x) / (end.x - start.x));
        if (end.y < padding.y) fraction = Mathf.Min(fraction, (padding.y - start.y) / (end.y - start.y));
        if (end.y > 1f - padding.y) fraction = Mathf.Min(fraction, (1f - padding.y - start.y) / (end.y - start.y));
        return Mathf.Clamp01(fraction);
    }

    private Vector2 ViewPadding()
    {
        if (sprite == null || arenaCamera == null || !arenaCamera.orthographic) return Vector2.zero;
        float height = arenaCamera.orthographicSize * 2f;
        return new Vector2(sprite.bounds.extents.x / (height * arenaCamera.aspect),
            sprite.bounds.extents.y / height);
    }
}
