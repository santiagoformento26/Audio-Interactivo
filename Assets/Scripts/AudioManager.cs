using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private AudioMixerSnapshot menuSnap, gameSnap, pauseSnap;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic, gameMusic;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        mixer.SetFloat("MusicCutoff", 22000f); // reset del filtro
        if (scene.name == "Menu") Play(menuMusic, menuSnap, 1.5f);
        if (scene.name == "Game") Play(gameMusic, gameSnap, 0.5f);
    }

    private void Play(AudioClip clip, AudioMixerSnapshot snap, float time)
    {
        if (musicSource.clip != clip) { musicSource.clip = clip; musicSource.loop = true; musicSource.Play(); }
        snap.TransitionTo(time);
    }

    public void SetPaused(bool paused) => (paused ? pauseSnap : gameSnap).TransitionTo(0.2f);

    // Bloque F: cuantas menos vidas, más "apagada" suena la música.
    public void OnLivesChanged(int lives)
    {
        float cutoff = lives >= 3 ? 22000f : lives == 2 ? 6000f : 1500f;
        mixer.SetFloat("MusicCutoff", cutoff); // parámetro expuesto en el grupo Música
    }
}