using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("좌우 이동")]
    [SerializeField] private float horizontalAmplitude = 0.5f; // 좌우로 움직이는 거리
    [SerializeField] private float horizontalSpeed = 1f;

    [Header("상하 이동")]
    [SerializeField] private float verticalAmplitude = 0.3f;   // 위아래로 움직이는 거리
    [SerializeField] private float verticalSpeed = 1.5f;

    [Header("옵션")]
    [SerializeField] private bool randomizePhase = true; // 여러 개 배치 시 움직임이 겹치지 않게

    private Vector3 startPos;
    private float phaseOffsetX;
    private float phaseOffsetY;
    private bool isFloating = true;

    void Awake()
    {
        startPos = transform.position;

        if (randomizePhase)
        {
            phaseOffsetX = Random.Range(0f, Mathf.PI * 2f);
            phaseOffsetY = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void Update()
    {
        if (!isFloating)
            return;

        float x = Mathf.Sin(Time.time * horizontalSpeed + phaseOffsetX) * horizontalAmplitude;
        float y = Mathf.Sin(Time.time * verticalSpeed + phaseOffsetY) * verticalAmplitude;

        transform.position = startPos + new Vector3(x, y, 0f);
    }

    public void SetFloating(bool floating)
    {
        isFloating = floating;
    }

    public void SetAnchor(Vector3 position)
    {
        startPos = position;
    }
}
