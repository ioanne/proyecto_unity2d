using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyFollowAndAnimate : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target;

    [Header("Movement Settings")]
    public float speed = 2f;
    public float followRange = 5f;
    public float stopRange = 0.1f;

    [Header("Line of Sight (optional)")]
    public bool requireLineOfSight = false;
    public LayerMask obstacleMask;

    [Header("Animation Settings")]
    public float movementThreshold = 0.01f; // Lower = more sensitive

    private Rigidbody2D rb;
    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (target == null)
        {
            StopMovement();
            return;
        }

        Vector2 direction = (target.position - transform.position);
        float distance = direction.magnitude;

        if (distance <= followRange && distance > stopRange && CanSeeTarget(direction, distance))
        {
            direction.Normalize();
            rb.linearVelocity = direction * speed;

            animator.SetFloat("moveX", direction.x);
            animator.SetFloat("moveY", direction.y);
            animator.SetBool("isMoving", true);
        }
        else
        {
            StopMovement();
        }
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("isMoving", false);
    }

    private bool CanSeeTarget(Vector2 direction, float distance)
    {
        if (!requireLineOfSight)
            return true;

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, obstacleMask);
        return hit.collider == null;
    }
}
