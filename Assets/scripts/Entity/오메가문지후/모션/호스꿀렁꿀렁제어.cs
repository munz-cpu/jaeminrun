using System.Collections.Generic;
using UnityEngine;

public class 호스꿀렁꿀렁제어 : MonoBehaviour
{
    [SerializeField] List<GameObject> pieces = new();

    [Header("꿀렁임 설정")]
    [SerializeField] float 속도 = 5f;
    [SerializeField] float 크기변화량 = 0.2f;
    [SerializeField] float 파츠간격 = 0.5f;

    private List<Vector3> 원래크기 = new();

    void Start()
    {
        foreach (GameObject piece in pieces)
        {
            원래크기.Add(piece.transform.localScale);
        }
    }

    void Update()
    {
        for (int i = 0; i < pieces.Count; i++)
        {
            // 파츠마다 타이밍을 조금씩 늦춤
            float 파동 = Mathf.Sin(
                Time.time * 속도 - i * 파츠간격
            );

            float 배율 = 1f + 파동 * 크기변화량;

            pieces[i].transform.localScale =
                원래크기[i] * 배율;
        }
    }
}