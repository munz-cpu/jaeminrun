using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class Battle3FlightController : MonoBehaviour
{
    [Header("Vertical Flight")]
    [SerializeField] private float riseSpeed = 7f;
    [SerializeField] private float fallSpeed = 6f;
    [SerializeField] private float acceleration = 18f;

    [Header("Flight Tilt")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float riseTiltAngle = 15f;
    [SerializeField] private float fallTiltAngle = -15f;
    [SerializeField] private float tiltSpeed = 8f;

    private Rigidbody2D rb;
    private float lockedX;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        lockedX = rb.position.x;

        if (visualTransform == null)
            visualTransform = transform;
    }

    private void FixedUpdate()
    {
        float targetVerticalSpeed = IsFlightPressed() ? riseSpeed : -fallSpeed;
        float verticalSpeed = Mathf.MoveTowards(
            rb.linearVelocity.y,
            targetVerticalSpeed,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(0f, verticalSpeed);
        rb.position = new Vector2(lockedX, rb.position.y);
    }

    private void Update()
    {
        float targetAngle = rb.linearVelocity.y >= 0f ? riseTiltAngle : fallTiltAngle;
        float angle = Mathf.LerpAngle(
            visualTransform.localEulerAngles.z,
            targetAngle,
            tiltSpeed * Time.deltaTime
        );

        visualTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private static bool IsFlightPressed()
    {
        bool mousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool touchPressed = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        bool keyboardPressed = Keyboard.current != null
            && (Keyboard.current.spaceKey.isPressed || Keyboard.current.wKey.isPressed);

        return mousePressed || touchPressed || keyboardPressed;
    }
}
