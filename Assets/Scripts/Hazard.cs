using UnityEngine;

// Clase base para todo elemento del nivel que pueda dañar o matar al jugador.
// Spikes y DeathZone heredan de esta clase (caso de Herencia).
public abstract class Hazard : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            ApplyEffect(collision.gameObject);
        }
    }

    // Cada peligro define su propio efecto sobre el jugador.
    protected abstract void ApplyEffect(GameObject player);
}
