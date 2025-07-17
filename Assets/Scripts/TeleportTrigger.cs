using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    [Tooltip("Referencia al objeto destino donde se moverá el jugador")]
    public Transform targetLocation;

    [Tooltip("Evita teleportar dos veces seguidas sin salir del trigger")]
    private bool hasTeleported = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasTeleported && other.CompareTag("Player"))
        {
            other.transform.position = targetLocation.position;
            hasTeleported = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hasTeleported = false;
        }
    }
}
