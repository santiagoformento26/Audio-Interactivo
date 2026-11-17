using UnityEngine;

// Zona de caída: al entrar el jugador solo suena el sonido, no muere ni pierde vidas.
public class FallZone : Hazard
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;         // Output: SFX
    [SerializeField] private AudioClip fallClip;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    private void Awake()
    {
        // Decisión 2D: el sonido no depende de la distancia a la zona.
        if (audioSource != null) audioSource.spatialBlend = 0f;
    }

    protected override void ApplyEffect(GameObject player)
    {
        if (audioSource == null || fallClip == null) return;

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(fallClip);
    }
}