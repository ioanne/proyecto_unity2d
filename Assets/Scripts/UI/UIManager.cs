using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject enemyHealthBar;
    [SerializeField] private GameObject playerHealthBar;
    [SerializeField] private GameObject doorInteractionMessage;

    private Enemy targetEnemy;
    private Coroutine hideBarCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void TogglePanel(GameObject panel)
    {
        if (panel != null)
        {
            bool isActive = panel.activeSelf;
            panel.SetActive(!isActive);
        }
    }

    public void ShowPanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    public void HidePanel(GameObject panel)
    {
        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    public void ShowEnemyHealthBar(Enemy enemy)
    {
        if (enemyHealthBar == null)
            return;

        // Si ya hay una barra activa y es de otro enemigo, ocultar la anterior
        if (targetEnemy != null && targetEnemy != enemy)
        {
            enemyHealthBar.SetActive(false);
            targetEnemy = null;

            if (hideBarCoroutine != null)
                StopCoroutine(hideBarCoroutine);
        }

        targetEnemy = enemy;
        enemyHealthBar.SetActive(true);
        UpdateEnemyHealthBar(enemy, enemy.CurrentHealth, enemy.MaxHealth);

        if (hideBarCoroutine != null)
            StopCoroutine(hideBarCoroutine);

        hideBarCoroutine = StartCoroutine(HideEnemyHealthBarAfterDelay(enemy, 5f));
    }

    public void UpdateEnemyHealthBar(Enemy enemy, int currentHealth, int maxHealth)
    {
        if (enemy == targetEnemy && enemyHealthBar != null)
        {
            Slider enemyHealthSlider = enemyHealthBar.GetComponentInChildren<Slider>();
            TextMeshProUGUI enemyHealthText = enemyHealthBar.GetComponentInChildren<TextMeshProUGUI>();

            enemyHealthSlider.maxValue = maxHealth;
            enemyHealthSlider.value = currentHealth;
            enemyHealthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    public void HideEnemyHealthBar(Enemy enemy)
    {
        if (targetEnemy == enemy && enemyHealthBar != null)
        {
            enemyHealthBar.SetActive(false);
            targetEnemy = null;

            if (hideBarCoroutine != null)
                StopCoroutine(hideBarCoroutine);
        }
    }

    private IEnumerator HideEnemyHealthBarAfterDelay(Enemy enemy, float delay)
    {
        yield return new WaitForSeconds(delay);
        HideEnemyHealthBar(enemy);
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (playerHealthBar != null)
        {
            Slider playerHealthSlider = playerHealthBar.GetComponent<Slider>();
            TextMeshProUGUI playerHealthText = playerHealthBar.GetComponentInChildren<TextMeshProUGUI>();
            playerHealthSlider.maxValue = maxHealth;
            playerHealthSlider.value = currentHealth;
            playerHealthText.text = $"{currentHealth}/{maxHealth}";
        }
    }

    public void ShowDoorInteractionMessage()
    {
        if (doorInteractionMessage != null)
        {
            doorInteractionMessage.SetActive(true);
        }
    }

    public void HideDoorInteractionMessage()
    {
        if (doorInteractionMessage != null)
        {
            doorInteractionMessage.SetActive(false);
        }
    }
}
