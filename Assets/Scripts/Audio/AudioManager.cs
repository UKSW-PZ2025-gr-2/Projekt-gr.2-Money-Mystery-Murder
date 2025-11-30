using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlaySFX(AudioClip clip)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    public void PlayMusic(AudioClip clip)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    public void SetVolume(float volume)
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }
}
