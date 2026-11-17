using UnityEngine;

public class Spikes : Hazard
{
    [SerializeField] private int damage = 1;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;         // Output: SFX
    [SerializeField] private AudioClip hitClip;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

    private void Awake()
    {
        if (audioSource != null) audioSource.spatialBlend = 0f;   // 2D
    }

    protected override void ApplyEffect(GameObject player)
    {
        if (audioSource != null && hitClip != null)
        {
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
            audioSource.PlayOneShot(hitClip);
        }

        IDamageable damageable = player.GetComponent<IDamageable>();
        damageable?.TakeDamage(damage);
    }
}