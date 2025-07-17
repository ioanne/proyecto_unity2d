using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIInventoryManager : MonoBehaviour
{
    public static UIInventoryManager Instance;

    [Header("Inventory UI")]
    [SerializeField] private TextMeshProUGUI keysText;
    [SerializeField] private TextMeshProUGUI keysText2;
    [SerializeField] private TextMeshProUGUI healthPotionsText;
    [SerializeField] private TextMeshProUGUI energyPotionsText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void UpdateInventoryUI(string itemName, int quantity)
    {
        switch (itemName)
        {
            case "Key":
                keysText.text = quantity.ToString();
                keysText2.text = quantity.ToString();
                break;
            case "HealthPotion":
                healthPotionsText.text = quantity.ToString();
                break;
            case "EnergyPotions":
                energyPotionsText.text = quantity.ToString();
                break;
            default:
                Debug.LogWarning($"Item {itemName} no tiene representaci�n en la UI.");
                break;
        }
    }

    // Este m�todo se asignar� al bot�n de la poci�n de vida
    public void OnHealthPotionButtonClicked()
    {
        // Restar una poci�n del inventario
        if (InventoryManager.Instance.UseItem("HealthPotion", 1))
        {
            // Curar al personaje
            Character player = FindObjectOfType<Character>();
            if (player != null)
            {
                player.Heal(20); // Suma 20 puntos de vida
            }
            else
            {
                Debug.LogWarning("No se encontr� el objeto Character en la escena.");
            }
        }
        else
        {
            Debug.Log("No tienes pociones de vida.");
        }
    }
}


