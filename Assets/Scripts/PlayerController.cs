using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int maxLives = 3;

    // Buena práctica: encapsulamiento con propiedad de solo lectura hacia afuera.
    public int Lives { get; private set; }

    private Rigidbody2D rb2D;
    private Animator animator;
    private float move;
    private bool isGrounded;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Lives = maxLives;
    }

    private void Update()
    {
        move = Input.GetAxisRaw("Horizontal");

        if (move != 0)
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
        }

        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("isGrounded", isGrounded);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        rb2D.linearVelocity = new Vector2(move * speed, rb2D.linearVelocity.y);
    }

    public void TakeDamage(int amount)
    {
        Lives -= amount;

        if (Lives <= 0)
        {
            GameManager.Instance.NotifyPlayerDied();
        }
    }
}
