using System.Collections;
using UnityEngine;

public class 동공지랄 : MonoBehaviour
{
    [SerializeField] private float 사이클1개걸리는시간 = 0.5f;
    [SerializeField] private Vector2 size = new Vector2(0.7f, 1.2f);

    float 절반;

    void Start()
    {
        절반 = 사이클1개걸리는시간 / 2f;
        StartCoroutine(동공확대축소());
    }

    IEnumerator 동공확대축소()
    {
        while (true)
        {
            // 작아짐
            yield return 크기변경(절반, size.x);

            // 커짐
            yield return 크기변경(절반, size.y);
        }
    }

    IEnumerator 크기변경(float n, float s)
    {
        Vector3 시작크기 = transform.localScale;
        Vector3 목표크기 = new Vector3(s, s, 시작크기.z);

        float 시간 = 0f;

        while (시간 < n)
        {
            시간 += Time.deltaTime;

            transform.localScale = Vector3.Lerp(
                시작크기,
                목표크기,
                시간 / n
            );

            yield return null;
        }

        transform.localScale = 목표크기;
    }
}