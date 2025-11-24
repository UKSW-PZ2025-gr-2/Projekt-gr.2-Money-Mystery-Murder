using System.Collections.Generic;
using UnityEngine;

public class InputConfigManager : MonoBehaviour
{
    public static InputConfigManager Instance { get; private set; }

    [SerializeField] private Dictionary<string, KeyCode> keyBindings = new();

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

    public bool GetKey(string actionName)
    {
        // TODO: Logic - wrapper for Input.GetKey based on bindings
        throw new System.NotImplementedException();
    }

    public void RebindKey(string actionName, KeyCode newKey)
    {
        // TODO: Logic - update dictionary and persist to PlayerPrefs
        throw new System.NotImplementedException();
    }

    public void LoadBindings()
    {
        // TODO: Logic - load saved bindings or initialize defaults
        throw new System.NotImplementedException();
    }

    public Dictionary<string, KeyCode> GetAllBindings()
    {
        return keyBindings;
    }
}
