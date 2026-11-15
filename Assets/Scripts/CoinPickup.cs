using UnityEngine;

public class CoinPickup : MonoBehaviour, ICollectible
{
    [SerializeField] private int value = 1;

    public int Value => value;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    public void Collect()
    {
        GameManager.Instance.RegisterCoinCollected();
        Destroy(gameObject);
    }
}
