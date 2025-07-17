
using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Maneja el sistema de salud y regeneración de cualquier entidad que reciba daño.
/// </summary>
public class HealthSystem
{
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private int maxHealth;
    private int currentHealth;

    private readonly float regenerationInterval;
    private readonly int regenerationAmount;
    private readonly MonoBehaviour coroutineOwner;

    private Coroutine regenerationCoroutine;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public bool IsDead => currentHealth <= 0;

    public HealthSystem(int maxHealth, MonoBehaviour owner, float regenInterval = 5f, int regenAmount = 1)
    {
        if (maxHealth <= 0)
            throw new ArgumentException("MaxHealth must be greater than zero");

        this.maxHealth = maxHealth;
        currentHealth = maxHealth;

        coroutineOwner = owner != null ? owner : throw new ArgumentNullException(nameof(owner));
        regenerationInterval = regenInterval;
        regenerationAmount = regenAmount;

        StartRegeneration();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            StopRegeneration();
            OnDeath?.Invoke();
        }
    }

    public void Heal(int healAmount)
    {
        if (IsDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void StartRegeneration()
    {
        if (regenerationAmount > 0 && regenerationInterval > 0)
        {
            regenerationCoroutine = coroutineOwner.StartCoroutine(RegenerateHealth());
        }
    }

    private void StopRegeneration()
    {
        if (regenerationCoroutine != null)
        {
            coroutineOwner.StopCoroutine(regenerationCoroutine);
            regenerationCoroutine = null;
        }
    }

    private IEnumerator RegenerateHealth()
    {
        while (!IsDead)
        {
            yield return new WaitForSeconds(regenerationInterval);
            Heal(regenerationAmount);
        }
    }
}
