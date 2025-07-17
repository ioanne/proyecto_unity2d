using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Configuración del Nivel")]
    [SerializeField] private string currentSceneName = "Level1"; // Nombre de esta escena
    [SerializeField] private string nextSceneName = "Level2";    // A qué escena ir si se gana
    [SerializeField] private bool isFinalLevel = false;
    [SerializeField] private string winSceneName = "WinScene";

    [Header("Configuración del chequeo")]
    [SerializeField] private float checkInterval = 1f;
    [SerializeField] private float delayBeforeTransition = 2f;

    private bool levelCompleted = false;

    private void Start()
    {
        InvokeRepeating(nameof(CheckForEnemies), checkInterval, checkInterval);
    }

    private void CheckForEnemies()
    {
        if (levelCompleted) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length == 0)
        {
            levelCompleted = true;
            Debug.Log("✅ Todos los enemigos eliminados.");

            Invoke(nameof(HandleLevelComplete), delayBeforeTransition);
        }
    }

    private void HandleLevelComplete()
    {
        if (isFinalLevel)
        {
            Debug.Log("🎉 Nivel final completado. Mostrando escena de victoria.");
            LoadSceneManager.instance.LoadSceneSynchronously(winSceneName);
        }
        else
        {
            Debug.Log($"➡ Cargando siguiente nivel: {nextSceneName}");
            LoadSceneManager.instance.LoadNextLevel(currentSceneName, nextSceneName);
        }
    }
}
