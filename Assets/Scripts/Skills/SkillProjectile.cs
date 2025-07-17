using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class SkillProjectile : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 3f; // Tiempo de vida si no choca con nada
    public string enemyTag = "Enemy";
    public string decorationTag = "Decoration"; // Añadido para la decoración

    private Rigidbody2D rb;
    private Animator animator;
    private bool hasExploded = false;

    void Awake()
    {
        // Obtenemos los componentes en Awake para asegurar que existan
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // El proyectil se destruirá después de 'lifetime' segundos si no ha chocado
        Destroy(gameObject, lifetime);
    }

    // Esta función ahora establece la dirección Y aplica la velocidad.
    // Debe ser llamada justo después de instanciar el proyectil.
    public void SetDirection(Vector2 direction)
    {
        // Normaliza la dirección para tener velocidad constante
        direction.Normalize();
        
        // Aplica la velocidad
        rb.velocity = direction * speed;

        // Opcional: Rotar el sprite para que mire en la dirección del movimiento
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Si ya hemos explotado, no hacemos nada más.
        if (hasExploded) return;

        // Comprobamos si el objeto con el que chocamos tiene una de las etiquetas deseadas
        // O si es un Tilemap con un collider.
        bool hitTarget = other.CompareTag(enemyTag) || 
                         other.CompareTag(decorationTag) ||
                         other.GetComponent<TilemapCollider2D>() != null;

        if (hitTarget)
        {
            hasExploded = true;

            // Cancelamos la autodestrucción por tiempo de vida, ya que ahora explotará.
            CancelInvoke(); 
            Destroy(gameObject, 2f); // Failsafe por si la animación no destruye el objeto

            // Detenemos el movimiento del proyectil
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // Evita que siga siendo afectado por físicas

            // Desactivamos el collider para no causar múltiples colisiones
            GetComponent<Collider2D>().enabled = false;

            // ¡La clave! Enviamos el trigger al Animator.
            Debug.Log($"💥 Impacto con {other.name}! Enviando trigger 'Explode'.");
            animator.SetTrigger("Explode");
        }
    }

    // Este método debe ser llamado por un "Animation Event" al final
    // del clip de animación "Explosion".
    public void OnExplosionFinished()
    {
        // Destruye el objeto del juego una vez la animación ha terminado.
        Destroy(gameObject);
    }
}
