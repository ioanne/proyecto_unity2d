using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]
public class SkillProjectile : MonoBehaviour
{
    [Header("Configuración general")]
    public float speed = 10f;
    public float lifetime = 30f;

    [Header("Colisiones")]
    [SerializeField] private string decorationTag = "Decoration"; // objetos a ignorar

    private string targetTag = "Enemy";         // objetivo válido para explotar
    private GameObject instigator = null;       // quien disparó (para no autogolpearse)

    private Rigidbody2D rb;
    private Animator animator;
    private AudioSource audioSource;

    private AudioClip hitSound;
    private bool hasExploded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // opcional
        audioSource = GetComponent<AudioSource>();
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(Vector2 direction)
    {
        direction.Normalize();
        rb.velocity = direction * speed;

        // Rotar visualmente
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetHitSound(AudioClip sound)
    {
        hitSound = sound;
    }

    public void SetTargetTag(string tag)
    {
        targetTag = tag;
    }

    public void SetInstigator(GameObject whoFired)
    {
        instigator = whoFired;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasExploded) return;

        if (collision.CompareTag(decorationTag))
        {
            Explode();
            return;
        }

        if (instigator != null && collision.gameObject == instigator)
            return;

        if (collision.CompareTag(targetTag))
        {
            int damage = 10;

            // Si el instigador tiene stats, los usamos
            // CharacterStats stats = instigator?.GetComponent<Player>()?.GetStats()
            //                         ?? instigator?.GetComponent<Enemy>()?.GetStats();

            // if (stats != null)
            //     damage = stats.Strength;
            damage = 10;

            // Aplicar daño al objetivo
            if (targetTag == "Enemy")
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null)
                    enemy.TakeDamage(damage);
            }
            else if (targetTag == "Player")
            {
                Player player = collision.GetComponent<Player>();
                if (player != null)
                    player.TakeDamage(damage);
            }

            Explode();
        }
    }

    private void Explode()
    {
        hasExploded = true;

        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        if (animator != null)
        {
            animator.SetTrigger("Explode");
            rb.velocity = Vector2.zero;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Llamado desde animación si existe
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
