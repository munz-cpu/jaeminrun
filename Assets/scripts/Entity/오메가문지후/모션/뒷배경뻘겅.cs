using UnityEngine;

public class 뒷배경뻘겅 : MonoBehaviour
{
    [SerializeField] private float 전환시간 = 1f;
    [SerializeField] private float 동작시간 = 5f;

    private float elapsed = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        Color color = spriteRenderer.color;

        if (elapsed >= 동작시간)
        {
            color.r = 0f;
            spriteRenderer.color = color;
            return;
        }

        float red = Mathf.PingPong(elapsed / 전환시간, 1f);

        color.r = red;
        spriteRenderer.color = color;
    }
}