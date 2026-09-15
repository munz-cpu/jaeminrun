using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class RunnerMovement : MonoBehaviour
{
    [Header("Horizontal Movement")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rightMoveSpeedMultiplier = 1.25f;
    [SerializeField] private float leftMoveSpeedMultiplier = 0.7f;
    [SerializeField] private BouncyRun2D bounceEffect;
    [SerializeField] private float rightBounceSpeedMultiplier = 1.4f;
    [SerializeField] private float leftBounceSpeedMultiplier = 0.75f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float doubleJumpForce = 10f;
    [SerializeField] private float gravityScale = 3f;
    [SerializeField] private float fallGravityMultiplier = 1.8f;
    [SerializeField] private AudioClip jumpSfx;

    [Header("Slam (구 슬라이드)")]
    [SerializeField] private float slamSpeed = 25f;        // 내려찍을 때 순간 속도
    [SerializeField] private float slamGravityScale = 8f;  // 슬램 중 중력 (더 빨리 떨어지게)

    [Header("Landing Impact")]
    [SerializeField] private float impactDuration = 0.2f;  // 착지 후 눌린 자세 유지 시간
    [SerializeField] private Vector2 normalColliderSize = new Vector2(0.8f, 1.6f);
    [SerializeField] private Vector2 impactColliderSize = new Vector2(1.2f, 0.6f);
    [SerializeField] private Vector2 normalColliderOffset = new Vector2(0f, 0.8f);
    [SerializeField] private Vector2 impactColliderOffset = new Vector2(0f, 0.3f);
    [SerializeField] private float impactRadius = 0.6f;    // 착지 충격 판정 범위 (장애물/적 타격용)
    [SerializeField] private LayerMask impactHitLayer;     // 슬램 착지 타격 대상 (필요 없으면 Nothing)
    [SerializeField] private AudioClip landingSfx;
    [SerializeField] private AudioClip landingSfx2;

    [Header("Camera Shake")]
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private float slamShakeDuration = 0.12f;
    [SerializeField] private float slamShakeStrength = 0.12f;

    [Header("Slam Tilt")]
    [SerializeField] private Transform visualTransform;
    [SerializeField] private float slamTiltAngle = -80f;   // 급강하 시 코를 박는 각도
    [SerializeField] private float tiltSpeed = 20f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private Animator anim;

    private bool isGrounded;
    private int jumpCount;
    private const int maxJumpCount = 2;

    private bool isSlamming;
    public bool IsSlamming => isSlamming;
    private bool isImpacting;
    private float impactTimer;

    private float targetTiltZ;
    private float moveInput;


    AudioSource ads;
    void Awake()
    {
        ads = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        //anim = GetComponent<Animator>();
        rb.gravityScale = gravityScale;

        if (cameraShake == null && Camera.main != null)
            cameraShake = Camera.main.GetComponent<CameraShake>();
        if (cameraShake == null)
            cameraShake = FindAnyObjectByType<CameraShake>();

        if (visualTransform == null)
            visualTransform = transform;

        if (bounceEffect == null)
            bounceEffect = GetComponentInChildren<BouncyRun2D>();


        UpdateBounceSpeed();

    }

    void Update()
    {
        GroundCheck();
        HandleGravity();
        HandleImpactTimer();
        HandleTilt();
    }

    private void FixedUpdate()
    {
        float speedMultiplier = moveInput > 0f
            ? rightMoveSpeedMultiplier
            : moveInput < 0f ? leftMoveSpeedMultiplier : 1f;

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed * speedMultiplier,
            rb.linearVelocity.y
        );
    }

    private void GroundCheck()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 공중 -> 지상 전이 순간
        if (isGrounded && !wasGrounded)
        {
            jumpCount = 0;

            if (isSlamming)
                Land();
        }
    }

    private void HandleGravity()
    {
        if (isSlamming)
        {
            rb.gravityScale = slamGravityScale;
            return;
        }

        if (rb.linearVelocity.y < 0f)
            rb.gravityScale = gravityScale * fallGravityMultiplier;
        else
            rb.gravityScale = gravityScale;
    }

    // ---------- Slam ----------

    private void StartSlam()
    {
        if (isSlamming || isGrounded) return;

        isSlamming = true;
        targetTiltZ = slamTiltAngle;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.down * slamSpeed, ForceMode2D.Impulse);

        //anim?.SetTrigger("Slam");
    }

    private void Land()
    {
        isSlamming = false;
        isImpacting = true;
        impactTimer = impactDuration;

        col.size = impactColliderSize;
        col.offset = impactColliderOffset;

        targetTiltZ = 0f;
        if (landingSfx) ads.PlayOneShot(landingSfx);
        if (landingSfx2) ads.PlayOneShot(landingSfx2);
        cameraShake?.Shake(slamShakeDuration, slamShakeStrength);

        //anim?.SetTrigger("SlamLand");

        // 착지 충격 범위 내 대상 타격 (필요 없으면 impactHitLayer를 Nothing으로 두면 자동 무효)
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, impactRadius, impactHitLayer);
        foreach (var hit in hits)
        {
            hit.GetComponentInParent<BossMinion>()?.HitBySlam();
        }
    }

    private void HandleImpactTimer()
    {
        if (!isImpacting) return;

        impactTimer -= Time.deltaTime;
        if (impactTimer <= 0f)
        {
            isImpacting = false;
            col.size = normalColliderSize;
            col.offset = normalColliderOffset;
        }
    }

    private void HandleTilt()
    {
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetTiltZ);
        visualTransform.localRotation = Quaternion.Lerp(
            visualTransform.localRotation,
            targetRot,
            Time.deltaTime * tiltSpeed
        );
    }

    // ---------- Input System 콜백 ----------

    public void OnMove(InputValue value)
    {
        moveInput = Mathf.Clamp(value.Get<float>(), -1f, 1f);
        UpdateBounceSpeed();
    }

    private void UpdateBounceSpeed()
    {
        if (bounceEffect == null)
            return;

        // 전투2에서는 이동 입력이 있을 때만 플레이어가 튄다.
        bounceEffect.SetRunning(gameObject.scene.name != "전투2" || !Mathf.Approximately(moveInput, 0f));

        float speedMultiplier = moveInput > 0f
            ? rightBounceSpeedMultiplier
            : moveInput < 0f ? leftBounceSpeedMultiplier : 1f;

        bounceEffect.SetSpeedMultiplier(speedMultiplier);
    }

    public void OnJump(InputValue value)
    {
        if (!value.isPressed) return;
        if (isSlamming) return; // 슬램 중엔 점프 무시

        if (jumpCount >= maxJumpCount) return;

        float force = jumpCount == 0 ? jumpForce : doubleJumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        if (jumpSfx) ads.PlayOneShot(jumpSfx);

        jumpCount++;

        //anim?.SetTrigger(jumpCount == 1 ? "Jump" : "DoubleJump");
    }

    public void OnCrouch(InputValue value) // 이벤트 이름은 그대로 Slide 유지
    {
        if (!value.isPressed) return;

        StartSlam(); // 땅에 있으면 내부에서 자동 무시됨
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, impactRadius);
    }
}
