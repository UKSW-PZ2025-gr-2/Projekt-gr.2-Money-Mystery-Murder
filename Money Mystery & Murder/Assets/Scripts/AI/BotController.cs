using UnityEngine;

public enum BotState
{
    Idle,
    Wander,
    Chase,
    Attack
}

[RequireComponent(typeof(Player))]
public class BotController : MonoBehaviour
{
    [SerializeField] private BotState currentState = BotState.Idle;

    public void UpdateState()
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }

    public void BuyRandomItem()
    {
        // TODO: Logic
        throw new System.NotImplementedException();
    }
}
