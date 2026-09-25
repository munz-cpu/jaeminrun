using System.Collections;
using UnityEngine;

public class 컷씬 : MonoBehaviour
{
    [SerializeField] private float 시작높이 = 5f;
    [SerializeField] private float 내려오는시간 = 2f;
    [SerializeField] private float 몇초뒤보여줌 = 3f;
    [SerializeField] private GameObject 오메가문지후본체;
    [SerializeField] private GameObject 캔버스;


    void Start()
    {
        Vector3 도착위치 = transform.position;
        Vector3 시작위치 = 도착위치 + Vector3.up * 시작높이;

        transform.position = 시작위치;

        StartCoroutine(내려오기(시작위치, 도착위치));
    }

    IEnumerator 내려오기(Vector3 시작위치, Vector3 도착위치)
    {
        float elapsed = 0f;

        while (elapsed < 내려오는시간)
        {
            elapsed += Time.deltaTime;

            float t = elapsed / 내려오는시간;

            transform.position = Vector3.Lerp(
                시작위치,
                도착위치,
                t
            );

            yield return null;
        }

        transform.position = 도착위치;

        // 여기다가 원하는 함수 실행
        StartCoroutine(보여줘());
    }
    IEnumerator 보여줘()
    {
        yield return new WaitForSeconds(몇초뒤보여줌);
        오메가문지후본체.SetActive(true);
        캔버스.SetActive(true);
        gameObject.SetActive(false);
    }
}