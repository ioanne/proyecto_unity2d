using UnityEngine;

public class EnemyRangedAttack : MonoBehaviour
{
    [SerializeField] private GameObject skillPrefab;
    [SerializeField] private float skillSpeed = 5f;
    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackWindupTime = 0.4f;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hitSound;

    private Transform player;
    private float cooldownTimer;
    private Animator animator;
    private AudioSource audioSource;
    private EnemyAnimatorControllerUpdater controllerUpdater;

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        controllerUpdater = GetComponent<EnemyAnimatorControllerUpdater>();
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (player == null || controllerUpdater == null) return;

        Vector2 toPlayer = (player.position - transform.position).normalized;

        if (cooldownTimer <= 0f &&
            controllerUpdater.IsTargetInRange() &&
            IsPlayerInFront(toPlayer))
        {
            FireSkill(toPlayer);
            cooldownTimer = attackCooldown;
        }
    }

    private bool IsPlayerInFront(Vector2 toPlayer)
    {
        Vector2 currentDir = toPlayer.normalized;
        float dot = Vector2.Dot(currentDir, toPlayer);
        return dot > 0.7f;
    }

    private void FireSkill(Vector2 direction)
    {
        if (controllerUpdater != null)
            controllerUpdater.SetAttackingState(true);

        if (animator != null)
            animator.SetTrigger("Attack");

        if (attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound);

        Invoke(nameof(LaunchProjectile), attackWindupTime);
        Invoke(nameof(EndAttack), attackWindupTime);
    }

    private void LaunchProjectile()
    {
        if (player == null) return;

        Vector2 direction = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 spawnPos = (Vector2)transform.position + direction * spawnOffset;

        GameObject skill = Instantiate(skillPrefab, spawnPos, Quaternion.identity);
        SkillProjectile projectile = skill.GetComponent<SkillProjectile>();

        if (projectile != null)
        {
            projectile.SetDirection(direction);
            projectile.speed = skillSpeed;
            projectile.SetTargetTag("Player");
            projectile.SetInstigator(gameObject);

            if (hitSound != null)
                projectile.SetHitSound(hitSound);
        }
        else
        {
            Debug.LogWarning("El prefab no tiene SkillProjectile.");
        }
    }

    private void EndAttack()
    {
        if (controllerUpdater != null)
            controllerUpdater.SetAttackingState(false);
    }
}
