using UnityEngine;

public class PlayerController : MonoBehaviour, IDamageable
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int maxLives = 3;

    [Header("Audio - Salto")]
    [SerializeField] private AudioSource jumpSource;          // Output: SFX
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private Vector2 jumpPitchRange = new Vector2(0.95f, 1.1f);
    [SerializeField] private Vector2 jumpVolumeRange = new Vector2(0.85f, 1f);

    [Header("Audio - Pasos")]
    [SerializeField] private AudioSource stepsSource;         // Output: SFX
    [SerializeField] private AudioClip[] stepClips;           // 3 o 4 clips alternados
    [SerializeField] private float stepInterval = 0.4f;
    [SerializeField] private Vector2 stepPitchRange = new Vector2(0.9f, 1.1f);
    [SerializeField] private Vector2 stepVolumeRange = new Vector2(0.6f, 0.8f);

    public int Lives { get; private set; }

    private Rigidbody2D rb2D;
    private Animator animator;
    private float move;
    private bool isGrounded;
    private float nextStepTime;
    private int lastStepIndex = -1;

    private void Awake()
    {
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        Lives = maxLives;

        // Decisión 2D: los sonidos del jugador no dependen de la distancia.
        if (jumpSource != null) jumpSource.spatialBlend = 0f;
        if (stepsSource != null) stepsSource.spatialBlend = 0f;
    }

    private void Update()
    {
        move = Input.GetAxisRaw("Horizontal");

        if (move != 0)
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb2D.linearVelocity = new Vector2(rb2D.linearVelocity.x, jumpForce);
            PlayVaried(jumpSource, jumpClip, jumpPitchRange, jumpVolumeRange);
        }

        HandleFootsteps();

        animator.SetFloat("Speed", Mathf.Abs(move));
        animator.SetFloat("VerticalVelocity", rb2D.linearVelocity.y);
        animator.SetBool("isGrounded", isGrounded);
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        rb2D.linearVelocity = new Vector2(move * speed, rb2D.linearVelocity.y);
    }

    // Pasos: solo suenan si el jugador está en el suelo y se mueve.
    private void HandleFootsteps()
    {
        if (isGrounded && move != 0 && Time.time >= nextStepTime)
        {
            PlayVaried(stepsSource, PickStepClip(), stepPitchRange, stepVolumeRange);
            nextStepTime = Time.time + stepInterval;
        }
    }

    // Elige un clip al azar sin repetir el anterior.
    private AudioClip PickStepClip()
    {
        if (stepClips == null || stepClips.Length == 0) return null;
        if (stepClips.Length == 1) return stepClips[0];

        int index;
        do { index = Random.Range(0, stepClips.Length); }
        while (index == lastStepIndex);

        lastStepIndex = index;
        return stepClips[index];
    }

    private void PlayVaried(AudioSource source, AudioClip clip, Vector2 pitchRange, Vector2 volumeRange)
    {
        if (source == null || clip == null) return;
        source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        source.PlayOneShot(clip, Random.Range(volumeRange.x, volumeRange.y));
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