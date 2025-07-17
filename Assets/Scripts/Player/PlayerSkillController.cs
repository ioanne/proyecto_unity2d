using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private float skillSpeed = 5f;
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;

    private PlayerController playerController;
    private AudioSource audioSource;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // clic izquierdo
        {
            Vector2 direction = playerController.GetLastDirection();

            if (direction == Vector2.zero)
                direction = Vector2.down; // dirección por defecto

            FireSkill(direction);
        }
    }

    void FireSkill(Vector2 direction)
    {
        // Sonido de ataque
        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }

        Vector2 spawnPosition = (Vector2)transform.position + direction.normalized * spawnOffset;

        GameObject skill = Instantiate(skillPrefab, spawnPosition, Quaternion.identity);
        SkillProjectile projectile = skill.GetComponent<SkillProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.speed = skillSpeed;
            projectile.SetTargetTag("Enemy");
            projectile.SetInstigator(gameObject);

            if (hitSound != null)
                projectile.SetHitSound(hitSound);
        }
        else
        {
            Debug.LogWarning("El prefab no tiene SkillProjectile.");
        }
    }
}
