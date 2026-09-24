using UnityEngine;
using UnityEngine.InputSystem;

public class 언더테일이동 : MonoBehaviour
{
    [SerializeField] Collider2D bound;
    [SerializeField] float speed = 0.7f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float delta = speed * Time.deltaTime;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            transform.position += new Vector3(0f,delta,0f);
        }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            transform.position += new Vector3(0f,-delta,0f);
        }
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            transform.position += new Vector3(-delta,0f,0f);
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            transform.position += new Vector3(delta,0f,0f);
        }

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, bound.bounds.min.x, bound.bounds.max.x);
        
        pos.y = Mathf.Clamp(pos.y, bound.bounds.min.y, bound.bounds.max.y);

        transform.position = pos;
    }
}
