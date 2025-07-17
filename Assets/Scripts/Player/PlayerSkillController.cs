using UnityEngine;

public class PlayerSkillController : MonoBehaviour
{
    public GameObject skillPrefab;
    public float skillSpeed = 5f;
    public float spawnOffset = 0.5f;

    private PlayerController playerController;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // clic izquierdo
        {
            Vector2 direction = playerController.GetLastDirection();

            if (direction == Vector2.zero)
                direction = Vector2.down; // por defecto

            FireSkill(direction);
        }
    }

    void FireSkill(Vector2 direction)
    {
        Vector2 spawnPosition = (Vector2)transform.position + direction.normalized * spawnOffset;

        GameObject skill = Instantiate(skillPrefab, spawnPosition, Quaternion.identity);
        SkillProjectile projectile = skill.GetComponent<SkillProjectile>();

        projectile.SetDirection(direction);
        projectile.speed = skillSpeed;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        skill.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
