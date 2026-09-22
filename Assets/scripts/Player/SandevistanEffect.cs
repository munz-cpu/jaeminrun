using UnityEngine;

public class SandevistanEffect : MonoBehaviour
{
    [SerializeField] float speedUp = 0.2f;
    [SerializeField] private GameObject afterImagePrefab;
    [SerializeField] private float spawnDelay = 0.05f;

    [Header("색상")]
    [SerializeField] private Color color1 = new Color(0.5f, 1f, 0.7f);
    [SerializeField] private Color color2 = new Color(0.5f, 0.8f, 1f);

    [SerializeField] private float colorSpeed = 2f;

    float speed;

    //PlayerMove playerMove;
    private SpriteRenderer spriteRenderer;
    private float timer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        //playerMove = GetComponent<PlayerMove>();
        //playerMove.speed *= speedUp + 1;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnDelay)
        {
            SpawnAfterImage();
            timer = 0f;
        }
    }

    void SpawnAfterImage()
    {
        GameObject afterImage =
            Instantiate(afterImagePrefab, transform.position, transform.rotation);

        SpriteRenderer afterImageRenderer =
            afterImage.GetComponent<SpriteRenderer>();

        // 플레이어 이미지 복사
        afterImageRenderer.sprite = spriteRenderer.sprite;
        afterImageRenderer.flipX = spriteRenderer.flipX;

        // 연초록 ↔ 연파랑 반복
        float t = Mathf.PingPong(
            Time.time * colorSpeed,
            1f
        );

        afterImageRenderer.color =
            Color.Lerp(color1, color2, t);
    }
}