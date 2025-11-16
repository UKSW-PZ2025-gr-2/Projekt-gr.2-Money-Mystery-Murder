using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private int playerCount = 1; // current number of players

    public int PlayerCount => playerCount;

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

    public void SetPlayerCount(int count)
    {
        if (count < 0) count = 0;
        playerCount = count;
    }

    public void IncrementPlayerCount() => playerCount++;
    public void DecrementPlayerCount() { if (playerCount > 0) playerCount--; }
}
