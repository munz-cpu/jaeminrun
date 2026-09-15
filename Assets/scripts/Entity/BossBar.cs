using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;

public class BossBar : MonoBehaviour
{
    [SerializeField] private Slider bossbar;
    
    EntityHealth bossHealth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bossHealth = GetComponent<EntityHealth>();
        bossbar.maxValue = bossHealth.maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        bossbar.value = bossHealth.currentHealth;
    }
}
