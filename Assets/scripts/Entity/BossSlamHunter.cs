using Unity.Tutorials.Editor;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class BossSlamHunter : MonoBehaviour
{
    public EscapeStageController stage;
    public Transform arm;
    public SpriteRenderer armRenderer;
    public SpriteRenderer warning;
    public SpriteRenderer bossRenderer;
    public CameraShake cameraShake;
    public float encounterStart = 18f;
    public float encounterEnd = 220f;
    [Min(0.1f)] public float waitingDuration = 4f;
    [Min(0.1f)] public float warningDuration = 1f;
    [Min(0.1f)] public float riseDuration = 0.5f;
    [Min(0.1f)] public float exposedDuration = 1.5f;
    [Min(0.1f)] public float fallDuration = 0.5f;
    [Min(0.01f)] public float slamDuration = 0.1f;
    public float groundY = -5.14f;
    [Min(0.1f)] public float visibleHeight = 8f;
    [Min(0f)] public float scanDistance = 1.4f;
    [Range(0f, 15f)] public float scanTilt = 5f;
    public Vector2 impactSize = new Vector2(3.2f, 3f);

    


    public string State { get; private set; } = "Waiting";
    public bool IsAttacking => State == "Warning" || State == "Rising" || State == "Exposed";
    public bool IsWarning => State == "Warning";
    public bool IsExposed => State == "Exposed";

    private float timer;
    private float hiddenY;
    private float exposedY;
    private float armBottomOffset;
    private Vector3 armRaised;
    private Vector3 armImpact;
    private bool slamStarted;
    private float slamElapsed;
    private float scanCenterX;
    private Quaternion restingRotation;
    private Camera viewCamera;
    [Header("효과음")]
    [SerializeField] AudioClip earthquake;
    [SerializeField] AudioClip detected;

    AudioSource audioSource;

    private void Start()
    {
        viewCamera = Camera.main;
        restingRotation = transform.rotation;
        if (!cameraShake) cameraShake = FindAnyObjectByType<CameraShake>();
        float spriteTopOffset = bossRenderer.bounds.max.y - transform.position.y;
        hiddenY = groundY - spriteTopOffset - 1f;
        exposedY = groundY + visibleHeight - spriteTopOffset;
        armBottomOffset = arm.position.y - armRenderer.bounds.min.y;
        HideVisuals();
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (stage.IsFinished)
        {
            if (State == "Exposed" && slamStarted)
            {
                AnimateSlam(Time.unscaledDeltaTime);
            }
            return;
        }
        float playerX = stage.player.transform.position.x;
        if (playerX < encounterStart || playerX > encounterEnd)
        { HideVisuals(); SetState("Waiting"); return; }

        timer += Time.deltaTime;
        switch (State)
        {
            case "Waiting":
                if (timer >= waitingDuration) BeginWarning();
                break;
            case "Warning":
                if (timer >= warningDuration) BeginRise();
                break;
            case "Rising":
                transform.position = new Vector3(transform.position.x,
                    Mathf.Lerp(hiddenY, exposedY, Mathf.Clamp01(timer / riseDuration)), transform.position.z);
                if (timer >= riseDuration) BecomeExposed();
                break;
            case "Exposed":
                ScanSurroundings();
                // A hidden player is never targeted; emerging from cover triggers the quick strike.
                if (!stage.CheckHidden()) Strike();
                if (slamStarted) AnimateSlam(Time.deltaTime);
                if (timer >= exposedDuration) BeginFall();
                break;
            case "Falling":
                transform.position = new Vector3(transform.position.x,
                    Mathf.Lerp(exposedY, hiddenY, Mathf.Clamp01(timer / fallDuration)), transform.position.z);
                if (timer >= fallDuration) { HideVisuals(); SetState("Waiting"); }
                break;
        }
    }

    private void BeginWarning()
    {
        if (earthquake)audioSource.PlayOneShot(earthquake);
        // Choose a position once per cycle; no horizontal movement while visible.
        float cameraX = viewCamera ? viewCamera.transform.position.x : stage.player.transform.position.x;
        scanCenterX = cameraX + 5f;
        transform.position = new Vector3(scanCenterX, hiddenY, transform.position.z);
        bossRenderer.enabled = true;
        cameraShake?.Shake(warningDuration, 0.3f);
        SetState("Warning");
    }

    private void BeginRise()
    {
        cameraShake?.Shake(riseDuration, 0.65f);
        SetState("Rising");
    }

    private void BecomeExposed()
    {
        transform.position = new Vector3(transform.position.x, exposedY, transform.position.z);
        SetState("Exposed");
        if (!stage.CheckHidden()) Strike();
    }

    private void ScanSurroundings()
    {
        float sweep = Mathf.Sin(2f * Mathf.PI * timer / exposedDuration);
        transform.position = new Vector3(scanCenterX + sweep * scanDistance,
            exposedY + Mathf.Abs(sweep) * 0.15f, transform.position.z);
        transform.rotation = restingRotation * Quaternion.Euler(0f, 0f, -sweep * scanTilt);
        bossRenderer.flipX = Mathf.Cos(2f * Mathf.PI * timer / exposedDuration) < 0f;
    }

    private void Strike()
    {
        if (slamStarted || stage.playerHealth.IsDead) return;
        if (detected) audioSource.PlayOneShot(detected);
        float targetX = stage.PlayerBounds.center.x;
        armImpact = new Vector3(targetX, groundY + armBottomOffset, arm.position.z);
        armRaised = armImpact + Vector3.up * 8f;
        arm.position = armRaised;
        armRenderer.enabled = true;
        slamStarted = true;
        slamElapsed = 0f;
        if (warning)
        {
            warning.transform.position = new Vector3(targetX, groundY + 0.12f, 0f);
            warning.enabled = true;
        }
        cameraShake?.Shake(0.25f, 1.1f);
        KillPlayer();
    }

    private void AnimateSlam(float deltaTime)
    {
        slamElapsed += deltaTime;
        float t = Mathf.Clamp01(slamElapsed / slamDuration);
        arm.position = Vector3.Lerp(armRaised, armImpact, t * t);
        if (t < 1f) return;
        slamStarted = false;
        if (warning) warning.enabled = false;
        cameraShake?.Shake(0.18f, 1.25f);
    }

    private void BeginFall()
    {
        armRenderer.enabled = false;
        if (warning) warning.enabled = false;
        SetState("Falling");
    }

    private void KillPlayer()
    {
        
        if (!stage.playerHealth.IsDead)
            stage.playerHealth.TakeDamage(stage.playerHealth.CurrentHealth);
    }

    private void SetState(string state) { State = state; timer = 0f; }

    private void HideVisuals()
    {
        transform.position = new Vector3(transform.position.x, hiddenY, transform.position.z);
        transform.rotation = restingRotation;
        bossRenderer.enabled = false;
        bossRenderer.flipX = false;
        armRenderer.enabled = false;
        if (warning) warning.enabled = false;
        slamStarted = false;
    }

    public void CancelAttack()
    {
        if (State == "Waiting" && !bossRenderer.enabled) return;
        HideVisuals();
        SetState("Waiting");
    }

    private void OnDisable()
    {
        if (bossRenderer) bossRenderer.enabled = false;
        if (armRenderer) armRenderer.enabled = false;
        if (warning) warning.enabled = false;
    }
}
