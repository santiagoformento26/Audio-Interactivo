using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Patrón Singleton: una única instancia accesible desde cualquier script (Spikes, DeathZone, CoinPickup, PlayerController).
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private TMP_Text coinsText;

    // Patrón Observer: cualquier script puede suscribirse a estos eventos sin que GameManager conozca a sus "oyentes".
    public event Action<int> OnCoinsChanged;
    public event Action OnPlayerDied;

    // Estructura de datos requerida: List, usada para registrar las monedas ya recolectadas.
    private List<int> collectedCoins = new List<int>();
    private int totalCoinsInLevel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        totalCoinsInLevel = FindObjectsByType<CoinPickup>(FindObjectsSortMode.None).Length;
        OnCoinsChanged += UpdateCoinsUI;
        Debug.Log($"[GameManager] Monedas totales detectadas en el nivel: {totalCoinsInLevel}");
    }

    public void RegisterCoinCollected()
    {
        collectedCoins.Add(1);
        OnCoinsChanged?.Invoke(collectedCoins.Count);
        Debug.Log($"[GameManager] Moneda recolectada. Total: {collectedCoins.Count}/{totalCoinsInLevel}");

        if (totalCoinsInLevel > 0 && collectedCoins.Count >= totalCoinsInLevel)
        {
            Debug.Log("[GameManager] Todas las monedas recolectadas, disparando Victory()");
            Victory();
        }
    }

    private void UpdateCoinsUI(int count)
    {
        if (coinsText != null)
            coinsText.text = count.ToString();
    }

    private bool isEnding;
    [SerializeField] private float endDelay = 1.5f;   // deja sonar el ultimo SFX antes de cambiar de escena

    public void NotifyPlayerDied()
    {
        if (isEnding) return;
        OnPlayerDied?.Invoke();
        Defeat();
    }

    public void Victory() => StartCoroutine(EndRoutine("Victory"));
    public void Defeat() => StartCoroutine(EndRoutine("Defeat"));

    private IEnumerator EndRoutine(string sceneName)
    {
        if (isEnding) yield break;
        isEnding = true;
        yield return new WaitForSecondsRealtime(endDelay);
        SceneManager.LoadScene(sceneName);
    }
}