using System.Collections;
using UnityEngine;

public abstract class Battle4Pattern : MonoBehaviour
{
    public abstract IEnumerator Execute();

    public virtual void Cancel() { }
}
