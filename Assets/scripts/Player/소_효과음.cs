using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class 소_효과음 : MonoBehaviour
{
    [Header("오디오 소스")]
    [SerializeField] private AudioClip[] audioClips; // 오디오 클립 배열

    [SerializeField] float minCooldown = 0.5f; // 최소 쿨다운 시간
    [SerializeField] float maxCooldown = 2f; // 최대 쿨다운
    
    private float cooldownTimer=0f; // 쿨다운 타이머

    AudioSource audioSource;    
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        cooldownTimer = Random.Range(minCooldown, maxCooldown); // 초기 쿨다운 설정
    }
    // Update is called once per frame
    void Update()
    {
        if (cooldownTimer <= 0f)
        {
            // 랜덤으로 오디오 클립 선택
            int randomIndex = Random.Range(0, audioClips.Length);
            AudioClip selectedClip = audioClips[randomIndex];

            // 오디오 재생
            audioSource.PlayOneShot(selectedClip);

            // 쿨다운 타이머 재설정
            cooldownTimer = Random.Range(minCooldown, maxCooldown);
        }
        else cooldownTimer -= Time.deltaTime;

    }
}
