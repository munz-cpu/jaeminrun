using UnityEngine;

public class 원을그리며이동 : MonoBehaviour
{
    [SerializeField] private float 반지름 = 1f;
    [SerializeField] private float 속도 = 2f;

    private Vector3 중심;

    void Start()
    {
        중심 = transform.localPosition;
    }

    void Update()
    {
        float 각도 = Time.time * 속도;

        float x = Mathf.Cos(각도) * 반지름;
        float y = Mathf.Sin(각도) * 반지름;

        transform.localPosition =
            중심 + new Vector3(x, y, 0f);
    }
}