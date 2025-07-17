using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    [Header("General Stats")]
    [SerializeField] private int maxHp = 100;
    [SerializeField] private int defense = 3;

    [Header("Attack Stats")]
    [SerializeField] private float attackPower = 10f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackSpeed = 2f;
    [SerializeField] private float regenerationInterval = 4f;
    [SerializeField] private int regenerationAmount = 1;
    [SerializeField] private AudioClip DeadSFX;

    public int CurrentHealth => healthSystem.CurrentHealth;
    public int MaxHealth => healthSystem.MaxHealth;

    private HealthSystem healthSystem;

    public event Action OnEnemyDeath;

    private void Awake()
    {
        healthSystem = new HealthSystem(maxHp, this, regenerationInterval, regenerationAmount);
        healthSystem.OnHealthChanged += UpdateHealthUI;
        healthSystem.OnDeath += Die;
    }

    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.Max(damage - defense, 0);
        healthSystem.TakeDamage(finalDamage);
        Debug.Log("Enemy Takes Damage: " + finalDamage);

        UIManager.Instance?.ShowEnemyHealthBar(this);
    }

    public bool IsDead() => healthSystem.CurrentHealth <= 0;

    private void Die()
    {
        // AudioManager.Instance.Playsound(DeadSFX);
        OnEnemyDeath?.Invoke();
        UIManager.Instance?.HideEnemyHealthBar(this);

        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        Destroy(gameObject, 0f);
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        UIManager.Instance?.UpdateEnemyHealthBar(this, currentHealth, maxHealth);
    }

    public float GetAttackPower() => attackPower;
    public float GetAttackCooldown() => attackCooldown;
    public float GetAttackSpeed() => attackSpeed;
}
