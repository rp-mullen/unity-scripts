using UnityEngine;

public class HealthComponent : VitalComponentBase
{
    // If you have loadout or stat modifiers that affect health,
    // override CalculateMaxValue here.
    protected override float CalculateMaxValue()
    {
        // For example, max health could be baseValue + (stat bonus from loadout)
        return baseValue; // Replace with your calculation logic.
    }

    // You can add health-specific methods or events here if needed.
}
