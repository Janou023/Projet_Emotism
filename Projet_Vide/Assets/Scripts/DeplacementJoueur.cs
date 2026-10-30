using UnityEngine;

public class DeplacementJoueur : MonoBehaviour
{
    public float moveSpeed = 3f;
    public Rigidbody2D rigidbody;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private Vector2 movement;

    public Sprite face;
    public Sprite dos;
    public Sprite profil;

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        movement = movement.normalized;

        animator.SetFloat("Speed", movement.sqrMagnitude);

        if (movement.x != 0)
        {
            spriteRenderer.sprite = profil;
            spriteRenderer.flipX = movement.x < 0;
        }
        if (movement.y > 0)
        {
            spriteRenderer.sprite = dos;
        }
        else if (movement.y < 0)
        {
            spriteRenderer.sprite = face;
        }
    }

    void FixedUpdate()
    {
        rigidbody.linearVelocity = movement * moveSpeed;
    }
}
