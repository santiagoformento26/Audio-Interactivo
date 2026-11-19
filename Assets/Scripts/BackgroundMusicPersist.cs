using UnityEngine;

public class BackgroundMusicPersist : MonoBehaviour
{
    public static BackgroundMusicPersist Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
