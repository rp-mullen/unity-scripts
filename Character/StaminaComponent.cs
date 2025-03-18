using UnityEngine;

public class StaminaComponent : VitalComponentBase
{
    protected override float CalculateMaxValue()
    {
        // For example, max stamina might factor in agility or endurance bonuses.
        return baseValue; // Replace with your calculation logic.
    }
}
