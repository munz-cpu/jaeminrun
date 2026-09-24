using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class 전투3뒷배경 : MonoBehaviour
{
    [SerializeField] private float distance = 10f;

    [SerializeField] private float sec = 45f;
    [SerializeField] private bool 왼쪽으로감 = true;
    
    float speed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = distance / sec;
        StartCoroutine(이동시작());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator 이동시작()
    {
        float elapsed = 0f;
        float delta;
        while (elapsed < distance)
        {
            delta = speed * Time.deltaTime * (왼쪽으로감?-1:1);
            transform.position += new Vector3(delta,0f,0f);
            elapsed += Math.Abs(delta);
            yield return null;
        }
    }
}
