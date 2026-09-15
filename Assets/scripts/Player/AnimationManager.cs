using UnityEngine;
using System.Collections.Generic;

public class AnimationManager : MonoBehaviour
{
    [SerializeField] private int currentSpriteNumber = 1;
    [SerializeField] private List<Sprite> images = new List<Sprite>();    


    private SpriteRenderer spriteRenderer;
    private 소_효과음 cowSound;
    public void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        cowSound = GetComponent<소_효과음>();
        ChangeSprite();
        
    }
    public void ChangeSprite(int spriteNumber = 0)
    {
        if (spriteNumber == 0)
        {
            spriteRenderer.sprite = images[currentSpriteNumber];
        }
        else
        {
            spriteRenderer.sprite = images[spriteNumber];
        }
        if (cowSound != null && currentSpriteNumber == 2)
        {
            cowSound.enabled = true;
        }
    }

}
