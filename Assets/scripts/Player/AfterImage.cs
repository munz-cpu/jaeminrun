using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField] private float lifeTime = 0.3f;
    [SerializeField] private float delta = -1f;
    private SpriteRenderer spriteRenderer;
    private float timer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        transform.Translate(Vector3.right * delta * Time.deltaTime);

        Color color = spriteRenderer.color;

        color.a = Mathf.Lerp(
            1f,
            0f,
            timer / lifeTime
        );

        spriteRenderer.color = color;

        if (timer >= lifeTime)
        {
            Destroy(gameObject);
        }
    }
}