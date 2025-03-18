using UnityEngine;

public class MagicComponent : VitalComponentBase
{
    protected override float CalculateMaxValue()
    {
        // For example, max magic might factor in intelligence or wisdom bonuses.
        return baseValue; // Replace with your calculation logic.
    }
}
