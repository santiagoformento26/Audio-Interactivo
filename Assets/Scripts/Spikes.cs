using UnityEngine;

public class Spikes : Hazard
{
    [SerializeField] private int damage = 1;

    protected override void ApplyEffect(GameObject player)
    {
        IDamageable damageable = player.GetComponent<IDamageable>();
        damageable?.TakeDamage(damage);
    }
}
