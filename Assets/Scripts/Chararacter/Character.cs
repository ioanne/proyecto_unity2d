using UnityEngine;

public class Character : MonoBehaviour
{
    private HealthSystem healthSystem;
    private CharacterStats characterStats;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int strength = 10;
    [SerializeField] private int defense = 5;
    [SerializeField] private float speed = 5f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float interactionRange = 4f; // Rango de interacción con la puerta
    [SerializeField] private float regenerationInterval = 3f; // Intervalo de regeneración
    [SerializeField] private int regenerationAmount = 2; // Cantidad de vida que se regenera
    [SerializeField] private AudioClip DamageTakeSFX;
    [SerializeField] private AudioClip DeadSFX;

    void Awake()
    {
        characterStats = new CharacterStats(strength, defense, speed, maxHealth);
        healthSystem = new HealthSystem(maxHealth, this, regenerationInterval, regenerationAmount);

        healthSystem.OnHealthChanged += UpdateHealthUI;
        healthSystem.OnDeath += HandleDeath;
    }

    void Update()
    {
    }

    public void TakeDamage(int damage)
    {
        AudioManager.Instance.Playsound(DamageTakeSFX);
        healthSystem.TakeDamage(damage);
    }
    
    private bool CheckIfEnemiesAreDead()
    {
        foreach (Enemy enemy in FindObjectsOfType<Enemy>())
        {
            Debug.Log($"Checking enemy {enemy.name}: IsDead = {enemy.IsDead()}");
            if (!enemy.IsDead()) return false;
        }
        return true;
    }

    public void Heal(int healAmount)
    {
        healthSystem.Heal(healAmount);
    }

    private void HandleDeath()
    {
        AudioManager.Instance.Playsound(DeadSFX);
        Debug.Log("Character has died.");
        string sceneName = "GameOverScene";
        LoadSceneManager.instance.LoadSceneSynchronously(sceneName);
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        // Verificar si el UIManager está disponible antes de actualizar la barra de vida
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            Debug.LogWarning("UIManager.Instance is null. Health UI could not be updated.");
        }

        // Debug.Log($"Health updated: {currentHealth}/{maxHealth}");
    }
}
