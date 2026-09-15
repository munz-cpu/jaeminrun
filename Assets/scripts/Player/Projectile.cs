using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [HideInInspector] public float speed = 5f;
    [HideInInspector] public float lifetime = 5f;
    [HideInInspector] public float damage = 10f;


    private Vector2 direction = Vector2.right;
    private float timer;
    private Action<Vector3> onEnemyHit;
    private bool consumed;

    public void Launch(
        Vector2 launchDirection,
        float launchSpeed,
        float launchLifetime,
        Action<Vector3> enemyHitCallback = null)
    {
        direction = launchDirection.normalized;
        speed = launchSpeed;
        lifetime = launchLifetime;
        onEnemyHit = enemyHitCallback;
        timer = 0f;
        consumed = false;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (consumed) return;

        // 플레이어가 발사한 팝콘은 플레이어 자신의 체력에 피해를 주지 않는다.
        if (other.GetComponentInParent<RunnerMovement>() != null)
        {
            return;
        }

        EntityHealth enemy = other.GetComponentInParent<EntityHealth>();
        if (enemy == null)
        {
            return;
        }

        consumed = true;
        BossMinion minion = enemy.GetComponent<BossMinion>();
        if (minion != null) minion.HitByPopcorn();
        else enemy.TakeDamage(damage);
        onEnemyHit?.Invoke(transform.position);
        onEnemyHit = null;
        Destroy(gameObject);
    }
}
