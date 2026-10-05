using System.Collections;
using UnityEngine;

/// <summary>
/// Reproduce voz, efectos y ambiente del juego. Si no hay un objeto "Audio" en la escena,
/// se crea solo (con sus tres AudioSource) la primera vez que se pide.
/// </summary>
public class AudioController : MonoBehaviour
{
    // Cuanto se baja musica y ambiente mientras habla un personaje (-6 dB aprox.)
    private const float DuckFactor = 0.5f;
    private const float AmbienceVolume = 1f;
    private const float FadeTime = 0.5f;

    private static AudioController instance;

    private AudioSource voiceSource;
    private AudioSource sfxSource;
    private AudioSource ambienceSource;
    private AudioSource musicSource;

    private bool isDucked;
    private Coroutine ambienceFade;
    private float fadeLevel = 1f;   // 0..1, nivel del fundido del ambiente
    private float musicVolume = 1f; // volumen base de la musica, sin atenuar

    public static AudioController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<AudioController>();
            }
            if (instance == null)
            {
                GameObject go = new GameObject("Audio");
                instance = go.AddComponent<AudioController>();
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        voiceSource = CreateSource("Voz", false);
        sfxSource = CreateSource("SFX", false);
        ambienceSource = CreateSource("Ambiente", true);
        // La musica puede venir de otro objeto de la escena; si no hay, se usa este.
        musicSource = CreateSource("Musica", true);
        ApplyVolumes();
    }

    private AudioSource CreateSource(string sourceName, bool loop)
    {
        GameObject go = new GameObject(sourceName);
        go.transform.SetParent(transform, false);
        AudioSource source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = 0f;
        return source;
    }

    /// <summary>Registra la fuente de musica de la escena para poder bajarla mientras hablan.</summary>
    public void SetMusicSource(AudioSource source)
    {
        if (source != null)
        {
            musicSource = source;
            musicVolume = source.volume;
            ApplyVolumes();
        }
    }

    private void Update()
    {
        // Si la voz termino sola, restaurar musica y ambiente.
        if (isDucked && !voiceSource.isPlaying)
        {
            Unduck();
        }
    }

    public void PlayVoice(AudioClip clip)
    {
        voiceSource.Stop();
        if (clip == null)
        {
            Unduck();
            return;
        }
        voiceSource.clip = clip;
        voiceSource.Play();
        Duck();
    }

    public void StopVoice()
    {
        voiceSource.Stop();
        Unduck();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>Cambia el ambiente con un fundido corto. Si viene null, se mantiene el actual.</summary>
    public void SetAmbience(AudioClip clip)
    {
        if (clip == null || ambienceSource.clip == clip)
        {
            return;
        }
        if (ambienceFade != null)
        {
            StopCoroutine(ambienceFade);
        }
        ambienceFade = StartCoroutine(FadeAmbience(clip));
    }

    private IEnumerator FadeAmbience(AudioClip clip)
    {
        float start = fadeLevel;
        for (float t = 0f; t < FadeTime && ambienceSource.isPlaying; t += Time.deltaTime)
        {
            fadeLevel = Mathf.Lerp(start, 0f, t / FadeTime);
            ApplyVolumes();
            yield return null;
        }
        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        fadeLevel = 0f;
        ApplyVolumes();
        ambienceSource.Play();
        for (float t = 0f; t < FadeTime; t += Time.deltaTime)
        {
            fadeLevel = Mathf.Lerp(0f, 1f, t / FadeTime);
            ApplyVolumes();
            yield return null;
        }
        fadeLevel = 1f;
        ApplyVolumes();
        ambienceFade = null;
    }

    /// <summary>Unico lugar que escribe el volumen: nivel del fundido x atenuacion por voz.</summary>
    private void ApplyVolumes()
    {
        float duck = isDucked ? DuckFactor : 1f;
        ambienceSource.volume = AmbienceVolume * fadeLevel * duck;
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * duck;
        }
    }

    private void Duck()
    {
        if (isDucked)
        {
            return;
        }
        isDucked = true;
        ApplyVolumes();
    }

    private void Unduck()
    {
        if (!isDucked)
        {
            return;
        }
        isDucked = false;
        ApplyVolumes();
    }
}
