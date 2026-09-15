using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float scrollSpeed = 3f;

    [Header("배경 스프라이트들 (왼쪽→오른쪽 순서로 등록)")]
    [SerializeField] private Transform[] backgrounds;

    [Header("카메라 (비워두면 Camera.main 사용)")]
    [SerializeField] private Camera cam;

    private float spriteWidth;

    void Start()
    {
        if (cam == null) cam = Camera.main;

        SpriteRenderer sr = backgrounds[0].GetComponent<SpriteRenderer>();
        spriteWidth = sr.bounds.size.x;
    }

    void Update()
    {
        foreach (Transform bg in backgrounds)
        {
            bg.position += Vector3.left * scrollSpeed * Time.deltaTime;
        }

        float camLeftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;

        foreach (Transform bg in backgrounds)
        {
            float bgRightEdge = bg.position.x + spriteWidth / 2f;

            // 배경의 오른쪽 끝이 카메라 왼쪽 끝을 완전히 벗어났을 때만 리셋
            if (bgRightEdge < camLeftEdge)
            {
                float rightmostX = GetRightmostX();
                bg.position = new Vector3(rightmostX + spriteWidth+20, bg.position.y, bg.position.z);
            }
        }
    }

    private float GetRightmostX()
    {
        float maxX = float.MinValue;
        foreach (Transform bg in backgrounds)
        {
            if (bg.position.x > maxX)
                maxX = bg.position.x;
        }
        return maxX;
    }
}