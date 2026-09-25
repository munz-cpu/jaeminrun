using System.Collections;
using UnityEngine;

public class Battle4PatternController : MonoBehaviour
{
    [SerializeField] private Battle4Pattern[] patterns;
    [SerializeField, Min(0f)] private float firstDelay = 1f;
    [SerializeField, Min(0f)] private float delayBetweenPatterns = 3f;

    private void OnEnable()
    {
        StartCoroutine(RunPatterns());
    }

    private IEnumerator RunPatterns()
    {
        if (patterns == null || patterns.Length == 0)
        {
            Debug.LogError("Battle4PatternController needs at least one pattern.", this);
            yield break;
        }

        yield return new WaitForSeconds(firstDelay);
        while (enabled)
        {
            foreach (Battle4Pattern pattern in patterns)
            {
                if (pattern == null || !pattern.isActiveAndEnabled) continue;
                yield return pattern.Execute();
                yield return new WaitForSeconds(delayBetweenPatterns);
            }
            yield return null;
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (patterns == null) return;
        foreach (Battle4Pattern pattern in patterns)
            if (pattern != null) pattern.Cancel();
    }
}
