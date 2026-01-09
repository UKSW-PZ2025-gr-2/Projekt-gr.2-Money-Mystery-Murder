using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Sound Effects")]
    public AudioClip footstepSound;
    public AudioClip moneySound;
    public AudioClip knifeSound;
    public AudioClip rifleShootSound;
    public AudioClip painSound;

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

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void SetVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }
    
    public void PlayFootstep() => PlaySFX(footstepSound);
    public void PlayMoney() => PlaySFX(moneySound);
    public void PlayKnife() => PlaySFX(knifeSound);
    public void PlayRifleShoot() => PlaySFX(rifleShootSound);
    public void PlayPain() => PlaySFX(painSound);
}
