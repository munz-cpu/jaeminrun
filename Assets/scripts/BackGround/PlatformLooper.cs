using UnityEngine;

public class PlatformLooper : MonoBehaviour
{
    [SerializeField] private Transform[] platforms;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private Camera targetCamera;

    private float platformWidth;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        SpriteRenderer sprite =
            platforms[0].GetComponentInChildren<SpriteRenderer>();

        platformWidth = sprite.bounds.size.x;
    }

    private void Update()
    {
        MovePlatforms();
        RecyclePlatforms();
    }

    private void MovePlatforms()
    {
        float movement = moveSpeed * Time.deltaTime;

        foreach (Transform platform in platforms)
            platform.position += Vector3.left * movement;
    }

    private void RecyclePlatforms()
    {
        float cameraLeft =
            targetCamera.ViewportToWorldPoint(Vector3.zero).x;

        foreach (Transform platform in platforms)
        {
            float platformRight =
                platform.position.x + platformWidth * 0.5f;

            if (platformRight < cameraLeft)
            {
                float rightmostX = platforms[0].position.x;

                foreach (Transform other in platforms)
                    rightmostX = Mathf.Max(rightmostX, other.position.x);

                platform.position = new Vector3(
                    rightmostX + platformWidth,
                    platform.position.y,
                    platform.position.z
                );
            }
        }
    }
}