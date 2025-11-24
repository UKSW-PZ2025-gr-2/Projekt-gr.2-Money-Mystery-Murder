using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [Header("Connection State")]
    [SerializeField] private bool isInitialized;
    [SerializeField] private string dbPath = "LocalStats.db"; // placeholder path

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

    void Start()
    {
        // Optionally auto initialize
        // InitializeDB();
    }

    public void InitializeDB()
    {
        // 1. Check if already initialized
        // 2. Open or create database file (pseudo - platform dependent)
        // 3. Create tables if not exist (PlayerStats, Achievements, etc.)
        // 4. Set isInitialized = true
        throw new System.NotImplementedException();
    }

    public void ExecuteQuery(string sql)
    {
        // 1. Validate isInitialized
        // 2. Prepare command
        // 3. Execute (non-query or query)
        // 4. Handle exceptions/logging
        throw new System.NotImplementedException();
    }
}
