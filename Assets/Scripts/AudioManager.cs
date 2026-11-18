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
        mixer.SetFloat("MusicCutoff", 22000f); // reset del filtro

        switch (scene.name)
        {
            case "Menu":
                Play(menuMusic, menuSnap, 1.5f);
                SetAmbient(false);
                break;

            case "Game":
                Play(gameMusic, gameSnap, 0.5f);
                SetAmbient(true);
                PlayVoice(startLine, voiceDelay);
                break;

            case "Victory":
                Play(victoryMusic, victorySnap != null ? victorySnap : menuSnap, 1f);
                SetAmbient(false);
                PlayVoice(victoryLine, voiceDelay);
                break;

            case "Defeat":
                Play(defeatMusic, defeatSnap != null ? defeatSnap : menuSnap, 2f);
                SetAmbient(false);
                PlayVoice(defeatLine, voiceDelay);
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
    }

    // ---------------- Bloque F: filtro segun vidas ----------------
    // Cuantas menos vidas, mas "apagada" suena la musica. Se llama desde PlayerController.TakeDamage.
    public void OnLivesChanged(int lives)
    {
        float cutoff = lives >= 3 ? 22000f : lives == 2 ? 6000f : 1500f;
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
        if (!mixer.GetFloat("MusicVolume", out float baseDb)) yield break;
        float duckedDb = baseDb + duckAmountDb;

        yield return FadeMusic(baseDb, duckedDb, duckFadeIn);
        yield return new WaitForSecondsRealtime(holdTime);
        yield return FadeMusic(duckedDb, baseDb, duckFadeOut);

        duckRoutine = null;
    }

    private IEnumerator FadeMusic(float from, float to, float time)
    {
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