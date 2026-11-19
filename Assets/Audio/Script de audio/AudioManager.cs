using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer mixer;

    [Header("Snapshots (Bloque B)")]
    [SerializeField] private AudioMixerSnapshot menuSnap;
    [SerializeField] private AudioMixerSnapshot gameSnap;
    [SerializeField] private AudioMixerSnapshot pauseSnap;
    [SerializeField] private AudioMixerSnapshot victorySnap;   // opcional: crear "Victoria" en el mixer
    [SerializeField] private AudioMixerSnapshot defeatSnap;    // opcional: crear "Derrota" en el mixer

    [Header("Musica (Output: Musica)")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private AudioClip victoryMusic;           // opcional
    [SerializeField] private AudioClip defeatMusic;            // opcional

    [Header("Ambiente (Bloque D, Output: Ambiente)")]
    [SerializeField] private AudioSource ambientSource;        // 2D, loop
    [SerializeField] private AudioClip ambientClip;            // oleaje / viento de playa

    [Header("Voz (Bloque E, Output: Voz)")]
    [SerializeField] private AudioSource voiceSource;          // 2D
    [SerializeField] private AudioClip startLine;
    [SerializeField] private AudioClip victoryLine;
    [SerializeField] private AudioClip defeatLine;
    [SerializeField] private float voiceDelay = 1f;            // espera a que termine la transicion del snapshot

    [Header("Ducking (Bloque F)")]
    [SerializeField] private float duckAmountDb = -10f;        // cuanto baja la musica cuando habla la voz
    [SerializeField] private float duckFadeIn = 0.15f;
    [SerializeField] private float duckFadeOut = 0.5f;

    private Coroutine duckRoutine;

    // Estado para que el filtro y el ducking sobrevivan a los TransitionTo (pausa, escenas).
    private int currentLives = 3;
    private bool isDucked;
    private float musicBaseDb;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Decision 2D: musica, ambiente y voz no dependen de la distancia.
        if (musicSource != null) musicSource.spatialBlend = 0f;
        if (ambientSource != null) { ambientSource.spatialBlend = 0f; ambientSource.loop = true; }
        if (voiceSource != null) voiceSource.spatialBlend = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ---------------- Bloque B: musica adaptativa por escena ----------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Un TransitionTo restaura los parametros expuestos, asi que cortamos cualquier ducking en curso.
        StopAllVoiceRoutines();
        isDucked = false;
        currentLives = 3;
        if (mixer != null) mixer.SetFloat("MusicCutoff", 22000f); // reset del filtro

        switch (scene.name)
        {
            case "Menu":
                Play(menuMusic, menuSnap, 1.5f);
                SetAmbient(false);
                musicSource.loop = true;
                break;

            case "Game":
                Play(gameMusic, gameSnap, 0.5f);
                SetAmbient(true);
                PlayVoice(startLine, voiceDelay);
                musicSource.loop = true;
                break;

            case "Victory":
                Play(victoryMusic, victorySnap != null ? victorySnap : menuSnap, 1f);
                SetAmbient(false);
                PlayVoice(victoryLine, voiceDelay);
                musicSource.loop = false;
                break;

            case "Defeat":
                Play(defeatMusic, defeatSnap != null ? defeatSnap : menuSnap, 2f);
                SetAmbient(false);
                PlayVoice(defeatLine, voiceDelay);
                musicSource.loop = false;
                break;
        }
    }

    private void Play(AudioClip clip, AudioMixerSnapshot snap, float time)
    {
        if (clip != null && musicSource.clip != clip)
        {
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
        if (snap != null) snap.TransitionTo(time);
    }

    private void SetAmbient(bool on)
    {
        if (ambientSource == null) return;
        if (on)
        {
            if (ambientSource.clip != ambientClip) ambientSource.clip = ambientClip;
            if (ambientClip != null && !ambientSource.isPlaying) ambientSource.Play();
        }
        else ambientSource.Stop();
    }

    // Se llama desde PauseManager.
    public void SetPaused(bool paused)
    {
        AudioMixerSnapshot snap = paused ? pauseSnap : gameSnap;
        if (snap != null) snap.TransitionTo(0.2f);

        // El TransitionTo pisa los parametros expuestos: al volver al juego los reaplicamos.
        if (!paused) StartCoroutine(ReapplyMusicParams());
    }

    private IEnumerator ReapplyMusicParams()
    {
        yield return new WaitForSecondsRealtime(0.25f);   // despues de que termine la transicion
        ApplyLivesFilter();
        if (isDucked) mixer.SetFloat("MusicVolume", musicBaseDb + duckAmountDb);
    }

    // ---------------- Bloque F: filtro segun vidas ----------------
    // Cuantas menos vidas, mas "apagada" suena la musica. Se llama desde PlayerController.TakeDamage.
    public void OnLivesChanged(int lives)
    {
        currentLives = lives;
        ApplyLivesFilter();
    }

    private void ApplyLivesFilter()
    {
        if (mixer == null) return;
        float cutoff = currentLives >= 3 ? 22000f : currentLives == 2 ? 6000f : 1500f;
        mixer.SetFloat("MusicCutoff", cutoff); // parametro expuesto en el grupo Musica
    }

    // ---------------- Bloque E + F: voz con ducking de la musica ----------------
    public void PlayVoice(AudioClip clip, float delay = 0f)
    {
        if (clip == null || voiceSource == null) return;
        StartCoroutine(VoiceRoutine(clip, delay));
    }

    private IEnumerator VoiceRoutine(AudioClip clip, float delay)
    {
        if (delay > 0f) yield return new WaitForSecondsRealtime(delay);

        voiceSource.Stop();
        voiceSource.clip = clip;
        voiceSource.Play();

        if (duckRoutine != null) StopCoroutine(duckRoutine);
        duckRoutine = StartCoroutine(DuckMusic(clip.length));
    }

    // Baja el volumen de la musica mientras suena la voz y lo devuelve despues.
    private IEnumerator DuckMusic(float holdTime)
    {
        // El volumen base se guarda una sola vez: si llega otra voz mientras esta bajada,
        // no se toma el valor ya bajado como si fuera el original.
        if (!isDucked)
        {
            if (!mixer.GetFloat("MusicVolume", out musicBaseDb)) yield break;
            isDucked = true;
        }
        float duckedDb = musicBaseDb + duckAmountDb;

        yield return FadeMusic(duckedDb, duckFadeIn);
        yield return new WaitForSecondsRealtime(holdTime);
        yield return FadeMusic(musicBaseDb, duckFadeOut);

        isDucked = false;
        duckRoutine = null;
    }

    // Fade desde el valor actual del mixer hasta "to".
    private IEnumerator FadeMusic(float to, float time)
    {
        mixer.GetFloat("MusicVolume", out float from);
        float t = 0f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            mixer.SetFloat("MusicVolume", Mathf.Lerp(from, to, t / time));
            yield return null;
        }
        mixer.SetFloat("MusicVolume", to);
    }

    private void StopAllVoiceRoutines()
    {
        StopAllCoroutines();
        duckRoutine = null;
        if (voiceSource != null) voiceSource.Stop();
    }
}