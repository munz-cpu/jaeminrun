using UnityEngine;

public class HutCover : MonoBehaviour
{
    public Vector2 size = new Vector2(4.8f, 2.8f);
    public Vector2 offset;
    public bool useSpriteBounds = true;
    private SpriteRenderer coverSprite;

    private void Awake() { coverSprite = GetComponent<SpriteRenderer>(); }

    private Bounds CoverBounds
    {
        get
        {
            if (useSpriteBounds && coverSprite && coverSprite.sprite) return coverSprite.bounds;
            return new Bounds(transform.position + (Vector3)offset, size);
        }
    }

    // Match the artwork rather than a narrower invisible box. Jumping above the roof exposes the player.
    public bool Conceals(Bounds playerBounds)
    {
        if (!isActiveAndEnabled) return false;
        Bounds cover = CoverBounds;
        return playerBounds.min.x >= cover.min.x + 0.15f &&
            playerBounds.max.x <= cover.max.x - 0.15f &&
            playerBounds.min.y >= cover.min.y - 0.35f &&
            playerBounds.max.y <= cover.max.y - 0.1f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (!coverSprite) coverSprite = GetComponent<SpriteRenderer>();
        Bounds cover = CoverBounds;
        Gizmos.DrawWireCube(cover.center, cover.size);
    }
}
