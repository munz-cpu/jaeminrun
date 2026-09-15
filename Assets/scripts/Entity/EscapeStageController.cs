using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EscapeStageController : MonoBehaviour
{
    public RunnerMovement player;
    public EntityHealth playerHealth;
    public BattleEndController result;
    public HutCover[] houses;
    public Transform ship;
    public Vector2 boardingOffset = new Vector2(-1.5f, -4.8f);
    public Vector2 boardingSize = new Vector2(3.5f, 3.5f);
    public Text statusText;
    public Text progressText;
    public Text instructionText;
    public BossSlamHunter boss;
    public bool IsBoarding { get; private set; }
    public bool IsHidden { get; private set; }
    public bool IsFinished => IsBoarding || result.IsEnding || playerHealth.IsDead;
    private Collider2D[] bodyColliders;
    private Rigidbody2D body;
    private float startX;
    private int lastPercent = -1;
    private string lastStatus;

    public Bounds PlayerBounds
    {
        get
        {
            Bounds bounds = bodyColliders[0].bounds;
            for (int i = 1; i < bodyColliders.Length; i++)
                if (bodyColliders[i].enabled && !bodyColliders[i].isTrigger) bounds.Encapsulate(bodyColliders[i].bounds);
            return bounds;
        }
    }

    private void Awake()
    {
        body = player.GetComponent<Rigidbody2D>();
        bodyColliders = player.GetComponentsInChildren<Collider2D>();
        startX = player.transform.position.x;
    }

    public bool CheckHidden()
    {
        Bounds bounds = PlayerBounds;
        foreach (var house in houses)
            if (house != null && house.Conceals(bounds)) return true;
        return false;
    }

    private void Update()
    {
        if (IsFinished) return;
        IsHidden = CheckHidden();
        int percent = Mathf.Clamp(Mathf.FloorToInt(100f * (player.transform.position.x-startX) / (ship.position.x-startX)), 0, 100);
        if (progressText && percent != lastPercent)
        { progressText.text = "우주선까지  " + percent + "%  →"; lastPercent = percent; }
        string status = boss.IsWarning ? "진동 경고! 초가집 뒤로 숨으세요" :
            boss.IsAttacking ? (IsHidden ? "보스가 보고 있습니다 · 집 밖으로 나가지 마세요" : "보스 등장! 초가집 뒤로 숨으세요") :
            IsHidden ? "초가집 뒤 · 안전" : "오른쪽 우주선으로 이동하세요";
        if (statusText && status != lastStatus)
        { statusText.text = status; statusText.color = boss.IsWarning ? new Color(1f, 0.75f, 0.35f) : boss.IsAttacking && !IsHidden ? new Color(1f, 0.4f, 0.35f) : IsHidden ? new Color(0.5f, 1f, 0.65f) : Color.white; lastStatus = status; }
        var boarding = new Bounds(ship.position + (Vector3)boardingOffset, boardingSize);
        if (boarding.Intersects(PlayerBounds)) StartCoroutine(BoardShip());
        else if (player.transform.position.y < -20f) playerHealth.TakeDamage(playerHealth.CurrentHealth);
    }

    private IEnumerator BoardShip()
    {
        IsBoarding = true;
        boss.CancelAttack();
        if (statusText) statusText.text = "탑승 완료! 우주선 출발";
        if (instructionText) instructionText.text = "탈출 성공";
        player.GetComponent<PlayerInput>()?.DeactivateInput();
        player.enabled = false;
        body.linearVelocity = Vector2.zero;
        body.simulated = false;
        foreach (var bounce in player.GetComponentsInChildren<BouncyRun2D>()) bounce.enabled = false;
        Vector3 from = player.transform.position;
        Vector3 destination = ship.position + (Vector3)boardingOffset;
        for (float t=0; t<0.6f; t+=Time.deltaTime)
        { player.transform.position = Vector3.Lerp(from, destination, t/0.6f); yield return null; }
        foreach (var renderer in player.GetComponentsInChildren<Renderer>()) renderer.enabled = false;
        for (float t=0; t<1.2f; t+=Time.deltaTime)
        { ship.position += Vector3.up * (2f + 10f*t) * Time.deltaTime; yield return null; }
        result.CompleteObjective();
    }

    private void OnDrawGizmosSelected()
    {
        if (!ship) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(ship.position + (Vector3)boardingOffset, boardingSize);
    }
}
