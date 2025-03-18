using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC : MonoBehaviour
{
    [Header("Character Setup")]
    // You can still reference a loadout for equipment, etc., if needed.
    public CharacterLoadout loadout;

    [Header("Faction Affiliations")]
    public List<Faction> affiliatedFactions;

    [Header("Interaction & Behavior")]
    public bool isInteractible = true;
    public bool isHostile = false;
    public float playerAffinity = 0f;

    [Header("Navigation & Schedule")]
    public NavMeshAgent navAgent;
    public ScheduleManager scheduleManager;

    [Header("Vital Components")]
    public HealthComponent healthComponent;
    public StaminaComponent staminaComponent;
    public MagicComponent magicComponent;

    protected virtual void Awake()
    {
        // Auto-assign components if not explicitly set.
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();
        if (staminaComponent == null) staminaComponent = GetComponent<StaminaComponent>();
        if (magicComponent == null) magicComponent = GetComponent<MagicComponent>();
        if (loadout == null) loadout = GetComponent<CharacterLoadout>();
    }

    protected virtual void Start()
    {
        // Subscribe to vital component events for UI updates or other feedback.
        if (healthComponent != null)
        {
            healthComponent.OnValueChanged += OnHealthChanged;
            healthComponent.OnDepleted += OnHealthDepleted;
        }
    }

    protected virtual void Update()
    {
        // Let the schedule manager update behavior based on time of day.
        if (scheduleManager != null)
        {
            scheduleManager.UpdateSchedule(Time.time);
        }

        // Additional common NPC behavior (patrolling, idling, etc.) could be added here.
    }

    protected virtual void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnValueChanged -= OnHealthChanged;
            healthComponent.OnDepleted -= OnHealthDepleted;
        }
    }

    protected virtual void OnHealthChanged(float newValue)
    {
        Debug.Log($"{gameObject.name} health changed: {newValue}");
        // Update UI or trigger effects as needed.
    }

    protected virtual void OnHealthDepleted()
    {
        Debug.Log($"{gameObject.name} has died.");
        // Handle NPC death here.
    }

    /// <summary>
    /// Triggers an interaction with this NPC.
    /// </summary>
    public virtual void Interact()
    {
        if (isInteractible)
        {
            Debug.Log($"{gameObject.name} is interacting with the player.");
            // Open dialogue, trigger quests, etc.
        }
    }

    /// <summary>
    /// Inflicts damage on the NPC by calling the HealthComponent.
    /// </summary>
    /// <param name="damage">Damage amount (should be positive).</param>
    public void TakeDamage(float damage)
    {
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(damage);
        }
    }

    /// <summary>
    /// Heals the NPC by calling the HealthComponent.
    /// </summary>
    /// <param name="amount">Healing amount (should be positive).</param>
    public void Heal(float amount)
    {
        if (healthComponent != null)
        {
            healthComponent.Replenish(amount);
        }
    }

    /// <summary>
    /// Consumes stamina using the StaminaComponent.
    /// </summary>
    public void ConsumeStamina(float amount)
    {
        if (staminaComponent != null)
        {
            staminaComponent.TakeDamage(amount);
        }
    }

    /// <summary>
    /// Recovers stamina using the StaminaComponent.
    /// </summary>
    public void RecoverStamina(float amount)
    {
        if (staminaComponent != null)
        {
            staminaComponent.Replenish(amount);
        }
    }

    /// <summary>
    /// Consumes magic using the MagicComponent.
    /// </summary>
    public void ConsumeMagic(float amount)
    {
        if (magicComponent != null)
        {
            magicComponent.TakeDamage(amount);
        }
    }

    /// <summary>
    /// Recovers magic using the MagicComponent.
    /// </summary>
    public void RecoverMagic(float amount)
    {
        if (magicComponent != null)
        {
            magicComponent.Replenish(amount);
        }
    }

    public Inventory GetInventory() {
        return loadout.inventory;
    }
}
