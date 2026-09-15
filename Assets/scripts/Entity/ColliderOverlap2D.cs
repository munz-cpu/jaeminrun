using UnityEngine;

public static class ColliderOverlap2D
{
    public static bool IsOverlapping(Collider2D first, Collider2D second)
    {
        if (first == null || second == null || !first.enabled || !second.enabled)
            return false;

        if (!first.gameObject.activeInHierarchy || !second.gameObject.activeInHierarchy)
            return false;

        // Collider2D.Distance compares the actual shapes directly, so it still works
        // when their layers are disabled in the Physics 2D collision matrix.
        return first.Distance(second).isOverlapped;
    }
}
