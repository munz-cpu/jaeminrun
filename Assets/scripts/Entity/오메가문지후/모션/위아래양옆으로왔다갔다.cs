using UnityEngine;

public class 위아래양옆으로왔다갔다 : MonoBehaviour
{
    [Header("위아래")]
    [SerializeField] private float 위아래거리 = 0.2f;
    [SerializeField] private float 위아래속도 = 5f;

    [Header("좌우")]
    [SerializeField] private float 좌우거리 = 0.2f;
    [SerializeField] private float 좌우속도 = 3f;

    private Vector3 시작위치;

    void Start()
    {
        시작위치 = transform.localPosition;
    }

    void Update()
    {
        float x = Mathf.Sin(Time.time * 좌우속도) * 좌우거리;
        float y = Mathf.Sin(Time.time * 위아래속도) * 위아래거리;

        transform.localPosition =
            시작위치 + new Vector3(x, y, 0f);
    }
}