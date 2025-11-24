using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMuzzleFlash(Vector3 position, Vector3 direction)
    {
        // TODO: Logic - spawn muzzle flash effect
        throw new System.NotImplementedException();
    }

    public void PlayBloodSplatter(Vector3 position)
    {
        // TODO: Logic - spawn blood splatter effect
        throw new System.NotImplementedException();
    }

    public void PlayExplosion(Vector3 position)
    {
        // TODO: Logic - spawn explosion effect
        throw new System.NotImplementedException();
    }
}
