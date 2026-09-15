using UnityEngine;

public class CloudScroller : MonoBehaviour
{
    [Header("씬에 있는 구름 3개")]
    [SerializeField] private Transform[] clouds = new Transform[3];

    [Header("구름 이미지")]
    [SerializeField] private Sprite[] cloudSprites = new Sprite[6];

    [Header("이동 속도")]
    [SerializeField] private Vector2 speedRange = new Vector2(0.5f, 1.5f);

    [Header("투명도")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.35f;

    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 0.8f;

    [Header("구름 사이 간격")]
    [SerializeField] private Vector2 respawnGapRange = new Vector2(2f, 5f);

    [Header("구름 크기")]  
    [SerializeField] private float minCloudSize = 0.5f;
    [SerializeField] private float maxCloudSize = 1.5f;

    [Header("카메라")]
    [SerializeField] private Camera targetCamera;

    private SpriteRenderer[] renderers;
    private float[] moveSpeeds;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        renderers = new SpriteRenderer[clouds.Length];
        moveSpeeds = new float[clouds.Length];

        for (int i = 0; i < clouds.Length; i++)
        {
            if (clouds[i] == null)
                continue;

            renderers[i] = clouds[i].GetComponent<SpriteRenderer>();

            if (renderers[i] == null)
            {
                Debug.LogError(
                    $"{clouds[i].name}에 SpriteRenderer가 없습니다.",
                    clouds[i]
                );

                continue;
            }

            SetRandomSpeed(i);
            SetRandomAlpha(i);
            SetRandomSprite(i);
            SetRandomSize(i);
            SetRandomPosition(i);
        }
    }

    private void Update()
    {
        if (targetCamera == null)
            return;

        float cameraLeft =
            targetCamera.transform.position.x
            - targetCamera.orthographicSize * targetCamera.aspect;

        for (int i = 0; i < clouds.Length; i++)
        {
            if (clouds[i] == null || renderers[i] == null)
                continue;

            clouds[i].position +=
                Vector3.left * moveSpeeds[i] * Time.deltaTime;

            if (renderers[i].bounds.max.x < cameraLeft)
                MoveToRight(i);
        }
    }

    private void MoveToRight(int cloudIndex)
    {
        float cameraRight =
            targetCamera.transform.position.x
            + targetCamera.orthographicSize * targetCamera.aspect;

        float rightmostPosition = cameraRight;

        for (int i = 0; i < clouds.Length; i++)
        {
            if (i == cloudIndex || renderers[i] == null)
                continue;

            rightmostPosition = Mathf.Max(
                rightmostPosition,
                renderers[i].bounds.max.x
            );
        }

        float gap = Random.Range(
            respawnGapRange.x,
            respawnGapRange.y
        );

        float targetLeftPosition = rightmostPosition + gap;
        float movement =
            targetLeftPosition - renderers[cloudIndex].bounds.min.x;

        clouds[cloudIndex].position += Vector3.right * movement;

        // 다시 나타날 때마다 이동 속도만 새로 결정합니다.
        SetRandomSpeed(cloudIndex);
    }

    private void SetRandomSpeed(int index)
    {
        moveSpeeds[index] = Random.Range(
            speedRange.x,
            speedRange.y
        );
    }

    private void SetRandomAlpha(int index)
    {
        Color color = renderers[index].color;

        color.a = Random.Range(
            Mathf.Min(minAlpha, maxAlpha),
            Mathf.Max(minAlpha, maxAlpha)
        );

        renderers[index].color = color;
    }

    private void SetRandomSprite(int index)
    {
        if (cloudSprites.Length == 0)
            return;

        int spriteIndex = Random.Range(0, cloudSprites.Length);
        renderers[index].sprite = cloudSprites[spriteIndex];
    }

    private void SetRandomPosition(int index)
    {
        if (targetCamera == null)
            return;

        float cameraLeft =
            targetCamera.transform.position.x
            - targetCamera.orthographicSize * targetCamera.aspect;

        float cameraRight =
            targetCamera.transform.position.x
            + targetCamera.orthographicSize * targetCamera.aspect;

        float randomX = Random.Range(cameraLeft, cameraRight);
        float randomY = clouds[index].position.y;

        clouds[index].position = new Vector3(randomX, randomY, 0f);
    }

    private void SetRandomSize(int index)
    {
        if (targetCamera == null)
            return;

        float cameraLeft =
            targetCamera.transform.position.x
            - targetCamera.orthographicSize * targetCamera.aspect;

        float cameraRight =
            targetCamera.transform.position.x
            + targetCamera.orthographicSize * targetCamera.aspect;

        float randomSize = Random.Range(
            minCloudSize,
            maxCloudSize
        );

        clouds[index].localScale = new Vector3(randomSize, randomSize, 1f);
    }
}