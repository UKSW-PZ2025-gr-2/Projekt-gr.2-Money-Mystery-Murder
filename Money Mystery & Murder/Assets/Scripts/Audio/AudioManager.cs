using UnityEngine;

/// <summary>
/// Manages all audio playback including music and sound effects.
/// Singleton pattern ensures only one instance exists across scene loads.
/// </summary>
public class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Singleton instance of the AudioManager.
    /// </summary>
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    /// <summary>
    /// AudioSource used for playing background music.
    /// </summary>
    public AudioSource musicSource;
    
    /// <summary>
    /// AudioSource used for playing sound effects.
    /// </summary>
    public AudioSource sfxSource;
    
    [Header("Sound Effects")]
    /// <summary>
    /// Sound effect for player footsteps.
    /// </summary>
    public AudioClip footstepSound;
    
    /// <summary>
    /// Sound effect for collecting money.
    /// </summary>
    public AudioClip moneySound;
    
    /// <summary>
    /// Sound effect for knife attacks.
    /// </summary>
    public AudioClip knifeSound;
    
    /// <summary>
    /// Sound effect for rifle shooting.
    /// </summary>
    public AudioClip rifleShootSound;
    
    /// <summary>
    /// Sound effect for taking damage/pain.
    /// </summary>
    public AudioClip painSound;

    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Ensures singleton pattern and persists across scene loads.
    /// </summary>
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure AudioSources exist
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Plays a sound effect once.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    /// <summary>
    /// Plays background music on loop.
    /// </summary>
    /// <param name="clip">The audio clip to play as music.</param>
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    /// <summary>
    /// Sets the global volume for all audio.
    /// </summary>
    /// <param name="volume">Volume level between 0 and 1.</param>
    public void SetVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }
    
    /// <summary>
    /// Plays the footstep sound effect.
    /// </summary>
    public void PlayFootstep() => PlaySFX(footstepSound);
    
    /// <summary>
    /// Plays the money collection sound effect.
    /// </summary>
    public void PlayMoney() => PlaySFX(moneySound);
    
    /// <summary>
    /// Plays the knife attack sound effect.
    /// </summary>
    public void PlayKnife() => PlaySFX(knifeSound);
    
    /// <summary>
    /// Plays the rifle shooting sound effect.
    /// </summary>
    public void PlayRifleShoot() => PlaySFX(rifleShootSound);
    
    /// <summary>
    /// Plays the pain/damage sound effect.
    /// </summary>
    public void PlayPain() => PlaySFX(painSound);
}
