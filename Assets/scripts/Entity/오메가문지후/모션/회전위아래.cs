using UnityEngine;

public class 회전위아래 : MonoBehaviour
{
    [Header("위아래 이동")]
    [SerializeField] private float 이동거리 = 0.2f;
    [SerializeField] private float 이동속도 = 5f;

    [Header("회전")]
    [SerializeField] private float 회전각도 = 15f;
    [SerializeField] private float 회전속도 = 5f;

    private Vector3 시작위치;
    private Quaternion 시작회전;

    void Start()
    {
        시작위치 = transform.localPosition;
        시작회전 = transform.localRotation;
    }

    void Update()
    {
        // -1 ~ 1 반복
        float 이동값 = Mathf.Sin(Time.time * 이동속도);
        float 회전값 = Mathf.Sin(Time.time * 회전속도);

        // 위 ↔ 아래
        transform.localPosition =
            시작위치 + Vector3.up * 이동값 * 이동거리;

        // 시계 ↔ 반시계
        transform.localRotation =
            시작회전 * Quaternion.Euler(0f, 0f, 회전값 * 회전각도);
    }
}