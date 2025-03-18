using System;
using UnityEngine;

public abstract class VitalComponentBase : MonoBehaviour
{
    [Header("Base Settings")]
    [Tooltip("The base maximum value (e.g., base health, base stamina, base magic) before loadout modifiers.")]
    public float baseValue = 100f;

    [SerializeField, Tooltip("Current value of the resource.")]
    protected float currentValue;

    /// <summary>
    /// Event triggered when the current value changes. Passes the new current value.
    /// </summary>
    public event Action<float> OnValueChanged;

    /// <summary>
    /// Event triggered when the resource is fully depleted (i.e. current value becomes 0).
    /// </summary>
    public event Action OnDepleted;

    /// <summary>
    /// Gets the current value.
    /// </summary>
    public float CurrentValue => currentValue;

    /// <summary>
    /// Gets the maximum value, calculated from the base value and optionally modified by loadout/stats.
    /// Override this to incorporate loadout data.
    /// </summary>
    public virtual float MaxValue => CalculateMaxValue();

    /// <summary>
    /// Calculates the maximum value for this resource.
    /// Derived classes can override this method to use loadout or stats data.
    /// </summary>
    /// <returns>The maximum value.</returns>
    protected virtual float CalculateMaxValue()
    {
        return baseValue;
    }

    /// <summary>
    /// Modifies the current value by the specified amount. Negative values mean damage/consumption,
    /// positive values mean replenishment.
    /// </summary>
    /// <param name="amount">The amount to change.</param>
    public virtual void ModifyValue(float amount)
    {
        float oldValue = currentValue;
        currentValue = Mathf.Clamp(currentValue + amount, 0, MaxValue);

        if (!Mathf.Approximately(currentValue, oldValue))
        {
            OnValueChanged?.Invoke(currentValue);
        }

        // If the resource has just been depleted, fire the event.
        if (currentValue == 0 && oldValue > 0)
        {
            OnDepleted?.Invoke();
        }
    }

    /// <summary>
    /// Inflicts damage (reduces the resource).
    /// </summary>
    /// <param name="damage">The amount of damage (should be positive).</param>
    public virtual void TakeDamage(float damage)
    {
        if (damage < 0)
        {
            Debug.LogWarning("Damage should be a positive value.");
            return;
        }
        ModifyValue(-damage);
    }

    /// <summary>
    /// Replenishes the resource (increases the current value).
    /// </summary>
    /// <param name="amount">The amount to replenish (should be positive).</param>
    public virtual void Replenish(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Replenish amount should be a positive value.");
            return;
        }
        ModifyValue(amount);
    }

    /// <summary>
    /// Resets the resource to its maximum value.
    /// </summary>
    public virtual void ResetToMax()
    {
        currentValue = MaxValue;
        OnValueChanged?.Invoke(currentValue);
    }

    /// <summary>
    /// Initializes the current value at startup.
    /// </summary>
    protected virtual void Start()
    {
        currentValue = MaxValue;
    }
}
