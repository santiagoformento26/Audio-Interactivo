using UnityEngine;

public class CoinPickup : MonoBehaviour, ICollectible
{
    [SerializeField] private int value = 1;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;         // Output: SFX
    [SerializeField] private AudioClip collectClip;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.15f);

    public int Value => value;

    private bool collected;

    private void Awake()
    {
        if (audioSource != null) audioSource.spatialBlend = 0f;   // 2D
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        if (collected) return;
        collected = true;

        // Ocultar la moneda y evitar nuevas colisiones mientras suena el clip.
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        foreach (Collider2D c in GetComponents<Collider2D>()) c.enabled = false;

        float destroyDelay = 0f;
        if (audioSource != null && collectClip != null)
        {
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            audioSource.PlayOneShot(collectClip);
            destroyDelay = collectClip.length / audioSource.pitch;
        }

        GameManager.Instance.RegisterCoinCollected();
        Destroy(gameObject, destroyDelay);
    }
}