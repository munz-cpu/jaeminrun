using UnityEngine;

public class 계속회전 : MonoBehaviour
{
    [SerializeField] int 방향 = 1;
    [SerializeField] public float 속도 = 360f;

    void Update()
    {
        transform.Rotate(0f, 0f, 속도 * 방향 * Time.deltaTime);
    }
}