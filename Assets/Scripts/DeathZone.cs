using UnityEngine;

public class DeathZone : Hazard
{
    protected override void ApplyEffect(GameObject player)
    {
        // Caer al vacío mata directamente, sin importar las vidas restantes.
        GameManager.Instance.NotifyPlayerDied();
    }
}
