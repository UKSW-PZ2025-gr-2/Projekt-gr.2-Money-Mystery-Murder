using UnityEngine;

public abstract class AbilityEffect : ScriptableObject
{
    // Called when an ability containing this effect is activated on the player
    public virtual void OnActivate(Player player) { }

    // Called every frame while the ability is active
    public virtual void OnTick(Player player, float deltaTime) { }

    // Called when the ability ends or is cancelled
    public virtual void OnDeactivate(Player player) { }

}
    