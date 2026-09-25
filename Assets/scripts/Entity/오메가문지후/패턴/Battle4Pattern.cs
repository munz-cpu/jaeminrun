using System.Collections;
using UnityEngine;

public abstract class Battle4Pattern : MonoBehaviour
{
    private AudioSource patternAudioSource;

    public abstract IEnumerator Execute();

    public virtual void Cancel() { }

    protected void PlayPatternSound(AudioClip clip)
    {
        if (clip == null) return;
        if (patternAudioSource == null)
        {
            patternAudioSource = GetComponent<AudioSource>();
            if (patternAudioSource == null)
                patternAudioSource = gameObject.AddComponent<AudioSource>();
        }
        patternAudioSource.PlayOneShot(clip);
    }
}
