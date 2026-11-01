using UnityEngine;

public class DeplacementJoueur : MonoBehaviour
{
    public float moveSpeed = 3f;
    public bool freeMovement;
    public Rigidbody2D rigidbody;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private Vector2 movement;

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (!freeMovement)
        {
            movement.y = 0;
        }

        movement = movement.normalized;

        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement.sqrMagnitude > 0.1f)
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
