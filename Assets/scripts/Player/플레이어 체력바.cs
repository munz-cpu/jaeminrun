using UnityEngine;
using UnityEngine.UI;

public class 플레이어체력바 : MonoBehaviour
{
    [SerializeField] private Slider playerbar;
    
    EntityHealth playerHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealth = GetComponent<EntityHealth>();
        playerbar.maxValue = playerHealth.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        playerbar.value = playerHealth.currentHealth;
    }
}
