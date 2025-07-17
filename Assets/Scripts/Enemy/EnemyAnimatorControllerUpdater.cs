using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyAnimatorControllerUpdater : MonoBehaviour
{
    public enum ControlMode { PlayerControlled, AutoByMovement }
    public ControlMode controlMode = ControlMode.AutoByMovement;

    [Header("Auto Follow Settings")]
    public string targetTag = "Player";     // Nuevo: buscar por tag
    public float followRange = 5f;
    public float speed = 2f;
    public bool requireLineOfSight = false;
    public LayerMask obstacleMask;

    public float movementThreshold = 0.05f;

    private Animator animator;
    private Rigidbody2D rb;
    private Transform target;

    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Buscar target si aún no está seteado
        if (target == null && !string.IsNullOrEmpty(targetTag))
        {
            GameObject found = GameObject.FindWithTag(targetTag);
            if (found != null)
            {
                target = found.transform;
            }
        }

        if (controlMode == ControlMode.PlayerControlled)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input.Normalize();

            animator.SetFloat("moveX", input.x);
            animator.SetFloat("moveY", input.y);
            animator.SetBool("isMoving", input.magnitude > 0);
        }
        else if (controlMode == ControlMode.AutoByMovement)
        {
            if (target != null && IsTargetVisible())
            {
                Vector2 direction = (target.position - transform.position).normalized;
                rb.velocity = direction * speed;
            }
            else
            {
                rb.velocity = Vector2.zero;
            }

            Vector2 velocity = rb.velocity;
            bool isMoving = velocity.magnitude > movementThreshold;

            if (isMoving)
            {
                Vector2 dir = velocity.normalized;
                animator.SetFloat("moveX", dir.x);
                animator.SetFloat("moveY", dir.y);
            }

            animator.SetBool("isMoving", isMoving);
        }
    }

    private bool IsTargetVisible()
    {
        if (target == null) return false;

        if (Vector2.Distance(transform.position, target.position) <= followRange)
        {
            if (requireLineOfSight)
            {
                Vector2 direction = (target.position - transform.position).normalized;
                float distance = Vector2.Distance(transform.position, target.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleMask);

                return hit.collider == null;
            }
            return true;
        }
        return false;
    }
}
