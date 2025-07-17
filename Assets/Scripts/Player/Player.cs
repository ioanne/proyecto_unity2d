using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Player : MonoBehaviour
{
    private HealthSystem healthSystem;
    private CharacterStats characterStats;

    [Header("Stats")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int strength = 10;
    [SerializeField] private int defense = 5;
    [SerializeField] private float speed = 5f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float interactionRange = 4f;
    [SerializeField] private float regenerationInterval = 3f;
    [SerializeField] private int regenerationAmount = 2;

    [Header("Audio")]
    [SerializeField] private AudioClip DamageTakeSFX;
    [SerializeField] private AudioClip DeadSFX;

    private void Awake()
    {
        characterStats = new CharacterStats(strength, defense, speed, maxHealth);
        healthSystem = new HealthSystem(maxHealth, this, regenerationInterval, regenerationAmount);

        healthSystem.OnHealthChanged += UpdateHealthUI;
        healthSystem.OnDeath += HandleDeath;
    }

    public void TakeDamage(int damage)
    {
        AudioManager.Instance.Playsound(DamageTakeSFX);
        healthSystem.TakeDamage(damage);
    }

    public void Heal(int healAmount)
    {
        healthSystem.Heal(healAmount);
    }

    private void HandleDeath()
    {
        AudioManager.Instance.Playsound(DeadSFX);
        Debug.Log("Player has died.");
        LoadSceneManager.instance.LoadSceneSynchronously("GameOverScene");
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning("UIManager.Instance is null. Health UI could not be updated.");
        }
    }

    public CharacterStats GetStats()
    {
        return characterStats;
    }

    public bool AreAllEnemiesDead()
    {
        foreach (Enemy enemy in FindObjectsByType<Enemy>(FindObjectsSortMode.None))
        {
            if (!enemy.IsDead()) return false;
        }
        return true;
    }
}
