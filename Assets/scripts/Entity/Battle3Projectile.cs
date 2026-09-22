using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Battle3Projectile : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 240f;

    private Battle3GameController gameController;
    private Vector2 moveDirection = Vector2.left;

    public void Initialize(Battle3GameController controller, Vector2 direction)
    {
        gameController = controller;
        moveDirection = direction.normalized;
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDirection * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        Camera gameCamera = gameController != null ? gameController.GameCamera : Camera.main;
        if (gameCamera != null && gameCamera.WorldToViewportPoint(transform.position).x < -0.2f)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Battle3FlightController>() == null)
            return;

        gameController?.GameOver();
        Destroy(gameObject);
    }
}
