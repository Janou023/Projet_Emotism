using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Déplacement")]
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;       // vue de dessus : pas de gravité
        rb.freezeRotation = true;   // évite que le joueur tourne en cas de collision
    }

    private void Update()
    {
        // Flèches directionnelles (et aussi WASD / ZQSD selon la config Input Manager)
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        // Normalisé pour que la diagonale ne soit pas plus rapide
        input = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = input * speed;   // Unity 6 et plus récent
#else
        rb.velocity = input * speed;         // Unity 2023.1 / 2023.2 et plus ancien
#endif
    }
}
