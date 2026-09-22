using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CrossfadeSlideshow : MonoBehaviour, IPointerClickHandler
{
    [Header("겹쳐 놓을 UI 이미지 두 개")]
    [Tooltip("처음 사진이 표시될 Image입니다.")]
    [SerializeField] private Image firstImage;
    [Tooltip("교차 페이드할 때 다음 사진이 표시될 Image입니다.")]
    [SerializeField] private Image secondImage;

    [Header("재생할 사진 목록")]
    [SerializeField] private List<Sprite> photos = new List<Sprite>();

    [Header("사진 순서와 같은 효과음 목록")]
    [Tooltip("Photos와 같은 순서로 넣으세요. 소리가 없는 사진은 빈 칸으로 두면 됩니다.")]
    [SerializeField] private List<AudioClip> sounds = new List<AudioClip>();

    [SerializeField] private AudioSource audioSource;

    [Header("재생 설정")]
    [Tooltip("사진 한 장이 완전히 보이는 시간입니다.")]
    [Min(0f)] [SerializeField] private float displayDuration = 3f;
    [Tooltip("두 사진이 겹쳐 보이며 바뀌는 시간입니다.")]
    [Min(0f)] [SerializeField] private float fadeDuration = 1f;
    [Tooltip("일시정지 중에도 사진을 계속 전환합니다.")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("마지막 사진 이후")]
    [InspectorName("다음 씬 이름 또는 경로")]
    [Tooltip("예: 전투1 또는 Assets/Scenes/전투1.unity. 비워두면 계속 반복합니다.")]
    [SerializeField] private string nextScenePath;

    private Coroutine slideshowCoroutine;
    private readonly List<Sprite> validPhotos = new List<Sprite>();
    private readonly List<int> photoSourceIndices = new List<int>();
    private Image currentImage;
    private Image nextImage;
    private int currentIndex;
    private bool skipWait;
    private bool isTransitioning;

    private void OnEnable()
    {
        StartSlideshow();
    }

    private void OnDisable()
    {
        StopSlideshow();
    }

    public void StartSlideshow()
    {
        StopSlideshow();

        if (!TryInitialize())
            return;

        if (validPhotos.Count > 1 || !string.IsNullOrWhiteSpace(nextScenePath))
            slideshowCoroutine = StartCoroutine(PlaySlideshow());
    }

    public void StopSlideshow()
    {
        skipWait = false;
        isTransitioning = false;

        if (audioSource != null)
            audioSource.Stop();

        if (slideshowCoroutine == null)
            return;

        StopCoroutine(slideshowCoroutine);
        slideshowCoroutine = null;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        NextPhoto();
    }

    // Button의 On Click에도 연결할 수 있습니다.
    public void NextPhoto()
    {
        if (!isActiveAndEnabled || slideshowCoroutine == null || isTransitioning)
            return;

        skipWait = true;
    }

    private bool TryInitialize()
    {
        if (firstImage == null || secondImage == null || firstImage == secondImage)
        {
            Debug.LogError("CrossfadeSlideshow에 서로 다른 UI Image 두 개를 연결해주세요.", this);
            return false;
        }

        validPhotos.Clear();
        photoSourceIndices.Clear();
        for (int i = 0; i < photos.Count; i++)
        {
            Sprite photo = photos[i];
            if (photo != null)
            {
                validPhotos.Add(photo);
                photoSourceIndices.Add(i);
            }
        }

        if (validPhotos.Count == 0)
        {
            SetAlpha(firstImage, 0f);
            SetAlpha(secondImage, 0f);
            return false;
        }

        currentIndex = 0;
        currentImage = firstImage;
        nextImage = secondImage;
        currentImage.sprite = validPhotos[currentIndex];
        nextImage.sprite = null;
        SetAlpha(currentImage, 1f);
        SetAlpha(nextImage, 0f);
        PlaySound(currentIndex);
        return true;
    }

    private IEnumerator PlaySlideshow()
    {
        while (true)
        {
            yield return Wait(displayDuration);
            skipWait = false;

            if (currentIndex == validPhotos.Count - 1 && !string.IsNullOrWhiteSpace(nextScenePath))
            {
                if (!Application.CanStreamedLevelBeLoaded(nextScenePath))
                {
                    Debug.LogError($"다음 씬 '{nextScenePath}'을(를) 로드할 수 없습니다. Build Settings에 씬을 추가하고 활성화해주세요.", this);
                    slideshowCoroutine = null;
                    yield break;
                }

                yield return SceneManager.LoadSceneAsync(nextScenePath);
                yield break;
            }

            isTransitioning = true;

            int nextIndex = (currentIndex + 1) % validPhotos.Count;
            nextImage.sprite = validPhotos[nextIndex];
            SetAlpha(nextImage, 0f);
            PlaySound(nextIndex);

            if (fadeDuration <= 0f)
            {
                SetAlpha(currentImage, 0f);
                SetAlpha(nextImage, 1f);
            }
            else
            {
                float elapsed = 0f;
                while (elapsed < fadeDuration)
                {
                    elapsed += DeltaTime;
                    float progress = Mathf.Clamp01(elapsed / fadeDuration);
                    SetAlpha(currentImage, 1f - progress);
                    SetAlpha(nextImage, progress);
                    yield return null;
                }
            }

            SetAlpha(currentImage, 0f);
            SetAlpha(nextImage, 1f);
            currentIndex = nextIndex;

            Image previousImage = currentImage;
            currentImage = nextImage;
            nextImage = previousImage;
            isTransitioning = false;
        }
    }

    private IEnumerator Wait(float duration)
    {
        if (duration <= 0f)
        {
            yield return null;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration && !skipWait)
        {
            elapsed += DeltaTime;
            yield return null;
        }
    }

    private float DeltaTime => useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

    private void PlaySound(int photoIndex)
    {
        int sourceIndex = photoSourceIndices[photoIndex];
        AudioClip clip = sourceIndex < sounds.Count ? sounds[sourceIndex] : null;

        if (clip == null)
        {
            if (audioSource != null)
                audioSource.Stop();
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
            }
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
    }

    private static void SetAlpha(Image image, float alpha)
    {
        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    private void OnValidate()
    {
        displayDuration = Mathf.Max(0f, displayDuration);
        fadeDuration = Mathf.Max(0f, fadeDuration);
    }
}
