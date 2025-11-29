using UnityEngine;

/// <summary>
/// Enum defining the different phases of gameplay.
/// </summary>
public enum GamePhase
{
    /// <summary>No phase set.</summary>
    None,
    /// <summary>Lobby/waiting phase before game starts.</summary>
    Lobby,
    /// <summary>Daytime phase.</summary>
    Day,
    /// <summary>Evening phase.</summary>
    Evening,
    /// <summary>Nighttime phase.</summary>
    Night,
    /// <summary>Game end phase.</summary>
    End
}

/// <summary>
/// Manages game phases and their transitions based on time of day.
/// Handles phase schedule configuration and phase change logic.
/// </summary>
public class PhaseManager : MonoBehaviour
{
    /// <summary>Current game phase (Day, Evening, Night, etc.). Set this in the Unity Inspector.</summary>
    [Header("Game Phase")]
    [SerializeField] private GamePhase currentPhase = GamePhase.Day;

    /// <summary>Hour (0-23) when Day phase begins. Set this in the Unity Inspector.</summary>
    [Header("Phase Schedule (Hours)")]
    [Range(0, 23)] [SerializeField] private int dayStartHour = 6;

    /// <summary>Hour (0-23) when Evening phase begins. Set this in the Unity Inspector.</summary>
    [Range(0, 23)] [SerializeField] private int eveningStartHour = 18;

    /// <summary>Hour (0-23) when Night phase begins. Set this in the Unity Inspector.</summary>
    [Range(0, 23)] [SerializeField] private int nightStartHour = 21;

    /// <summary>Gets the current game phase.</summary>
    public GamePhase CurrentPhase => currentPhase;

    /// <summary>Gets the day start hour.</summary>
    public int DayStartHour => dayStartHour;

    /// <summary>Gets the evening start hour.</summary>
    public int EveningStartHour => eveningStartHour;

    /// <summary>Gets the night start hour.</summary>
    public int NightStartHour => nightStartHour;

    /// <summary>Initializes the phase schedule and validates configuration.</summary>
    public void Initialize()
    {
        eveningStartHour = Mathf.Clamp(eveningStartHour, 0, 23);
        nightStartHour = Mathf.Clamp(nightStartHour, 0, 23);
        dayStartHour = Mathf.Clamp(dayStartHour, 0, 23);
        
        if (eveningStartHour <= dayStartHour) eveningStartHour = Mathf.Min(23, dayStartHour + 1);
        if (nightStartHour <= eveningStartHour) nightStartHour = Mathf.Min(23, eveningStartHour + 1);

        if (currentPhase == GamePhase.None || currentPhase == GamePhase.Lobby)
        {
            currentPhase = GamePhase.Day;
        }
    }

    /// <summary>
    /// Determines the appropriate game phase based on the given hour.
    /// </summary>
    /// <param name="hour">The hour (0-23) to evaluate.</param>
    /// <returns>The corresponding <see cref="GamePhase"/>.</returns>
    public GamePhase DeterminePhaseByTime(int hour)
    {
        if (hour >= nightStartHour || hour < dayStartHour) return GamePhase.Night;
        if (hour >= eveningStartHour) return GamePhase.Evening;
        return GamePhase.Day;
    }

    /// <summary>
    /// Sets the current game phase.
    /// </summary>
    /// <param name="phase">The <see cref="GamePhase"/> to set.</param>
    public void SetPhase(GamePhase phase)
    {
        if (currentPhase == phase) return;
        
        currentPhase = phase;
        Debug.Log($"[PhaseManager] Phase set to {currentPhase}");

        switch (phase)
        {
            case GamePhase.Day:
                StartDay();
                break;
            case GamePhase.Evening:
                StartEvening();
                break;
            case GamePhase.Night:
                StartNight();
                break;
        }
    }

    /// <summary>
    /// Changes the current game phase to a new state.
    /// </summary>
    /// <param name="newState">The new <see cref="GamePhase"/> to transition to.</param>
    public void ChangePhase(GamePhase newState)
    {
        SetPhase(newState);
    }

    /// <summary>Advances the game phase to the next sequential phase.</summary>
    public void AdvancePhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Day: 
                SetPhase(GamePhase.Evening); 
                break;
            case GamePhase.Evening: 
                SetPhase(GamePhase.Night); 
                break;
            case GamePhase.Night: 
                SetPhase(GamePhase.Day); 
                break;
            case GamePhase.End: 
                SetPhase(GamePhase.Day); 
                break;
            default: 
                SetPhase(GamePhase.Day); 
                break;
        }
        Debug.Log($"[PhaseManager] Phase advanced to {currentPhase}");
    }

    /// <summary>
    /// Updates phase based on current time if needed.
    /// </summary>
    /// <param name="currentHour">The current hour (0-23).</param>
    public void UpdatePhaseByTime(int currentHour)
    {
        var phaseByTime = DeterminePhaseByTime(currentHour);
        if (phaseByTime != currentPhase)
        {
            SetPhase(phaseByTime);
        }
    }

    /// <summary>Starts the day phase.</summary>
    public void StartDay()
    {
        Debug.Log("[PhaseManager] Day phase started");
    }

    /// <summary>Starts the evening phase.</summary>
    public void StartEvening()
    {
        Debug.Log("[PhaseManager] Evening phase started");
    }

    /// <summary>Starts the night phase.</summary>
    public void StartNight()
    {
        Debug.Log("[PhaseManager] Night phase started");
    }
}
