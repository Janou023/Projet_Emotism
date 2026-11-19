using UnityEngine;

public class DeplacementJoueur : MonoBehaviour
{
    public float moveSpeed = 3f;
    public bool freeMovement;
    public Rigidbody2D rigidbody;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private Vector2 movement;

    void Awake()
    {
        if (LoadDefi.TryGetReturnPosition(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, out Vector3 position))
        {
            transform.position = position;
            if (rigidbody != null)
                rigidbody.position = position;
        }
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (!freeMovement)
        {
            movement.y = 0;
        }

        movement = Vector2.ClampMagnitude(movement, 1f);

        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement.sqrMagnitude > 0.01f)
        {
            spriteRenderer.flipX = movement.x < 0;
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
        }
    }

    void FixedUpdate()
    {
        rigidbody.linearVelocity = movement * moveSpeed;
    }
}
