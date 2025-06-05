using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spells/Spell")]
[System.Serializable]
public class Spell : ScriptableObject
{
    [Header("Spell Info")]
    public string spellName;
    public string description;
    public int manaCost;
    public float castTime;
    public float cooldown;

[Header("Spell Attributes")]
    public SpellType spellType;
    public SpellShape spellShape;
    public MagicSchool magicSchool;
    
    [Header("Visual FX")]
    public GameObject spellPrefab;

    [Header("Targeting")]
    public SpellTargeting targetingType;
    public DamageType damageType;
    public bool canBeHeld = false;
    public bool requiresLineOfSight;
    public LayerMask targetLayers;

    [Header("Scaling & Effects")]
    public float powerScaling; 
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    [Header("Spell Commands")]
    public List<SpellCommand> spellCommands = new List<SpellCommand>();


    [Header("Visual Settings")]
    public string displayChild;
    public List<ObjectBlend> blendShapes;

    public float aoeRadius;
    public float baseDamage;
    public float healAmount;
    public float baseBuffValue;
    public float effectDuration;

    public AnimatorOverrideController animatorOverride;
    
public void Equip(CharacterStats character)
{
    if (character == null || character.owner == null) return;

    Debug.Log($"{spellName} equipped.");

}

public void Unequip(CharacterStats character)
{
    if (character == null || character.owner == null) return;

    Debug.Log($"{spellName} unequipped.");
}

public void Cast(CharacterStats casterStats, HandType handType, Transform overrideOrigin = null)
{

    if (casterStats == null || casterStats.owner == null)
    {
        Debug.LogWarning($"Spell {spellName} could not be cast — missing caster.");
        return;
    
        }

    var magic = casterStats?.owner?.transform.FindDeepChildFromRoot<CharacterLoadout>().magicComponent;
        if (magic == null || magic.CurrentValue < manaCost)
        {
            Debug.Log("Not enough mana to cast.");
            return;
        }
        else {
            magic.TakeDamage(manaCost);
        }
    Transform castOrigin = overrideOrigin;

    if (castOrigin == null)
    {
        var handBone = handType == HandType.Left ? "mixamorig:LeftHand" : "mixamorig:RightHand";
        castOrigin = FindBoneTransform(casterStats.owner.transform, handBone);
        if (castOrigin == null)
        {
            Debug.LogWarning($"Spell {spellName} could not find cast origin (mixamorig:RightHand) on {casterStats.owner.name}");
            return;
        }
    }

    if (spellPrefab == null)
    {
        Debug.LogWarning($"Spell {spellName} has no prefab assigned.");
        return;
    }

    Vector3 direction = Vector3.forward;
    var playerState = casterStats.owner.GetComponent<PlayerAnimatorController>();

    if (playerState != null)
    {
        if (playerState.isLockedOn && playerState.lockedTarget != null)
        {
            Vector3 toTarget = FindBoneTransform(playerState.lockedTarget.transform,"mixamorig:Spine1").position - castOrigin.position;
            direction = toTarget.normalized;
            Debug.Log($"[SpellCast] Lock-on active. Aiming at {playerState.lockedTarget.name} with vertical aim.");
        }
        else if (playerState.lastMoveDirection.sqrMagnitude > 0.01f)
        {
            direction = playerState.lastMoveDirection.normalized;
            Debug.Log("[SpellCast] Using last movement direction.");
        }
        else
        {
            direction = casterStats.owner.transform.forward;
            Debug.LogWarning("[SpellCast] No movement input — defaulting to forward.");
        }
    }
    else
    {
        direction = casterStats.owner.transform.forward;
        Debug.LogWarning("[SpellCast] PlayerAnimatorController not found — using forward.");
    }

    Quaternion rotation = Quaternion.LookRotation(direction);
    GameObject spawned = GameObject.Instantiate(spellPrefab, castOrigin.position, rotation);

    string handParticlesName = handType == HandType.Left ? "HandParticlesL" : "HandParticlesR";
    Transform handParticles = FindBoneTransform(casterStats.owner.transform, handParticlesName);


    var projectile = spawned.GetComponent<SpellProjectile>();
    if (projectile != null)
    {
        projectile.caster = casterStats;
        projectile.damage = baseDamage * powerScaling;
        projectile.damageType = damageType;
        projectile.statusEffects = new List<StatusEffect>(statusEffects);
        projectile.targetLayers = targetLayers;
        projectile.aoeRadius = aoeRadius;
        projectile.direction = direction;
        projectile.sourceSpell = this;
    }

    // Apply color from hand particle system
    if (handParticles != null)
    {
        projectile.ApplyHandParticleColor(handParticles);
    }

    Debug.Log($"Spell {spellName} cast.");
}

private Transform FindBoneTransform(Transform root, string boneName)
{
    Transform[] children = root.GetComponentsInChildren<Transform>(true);
    foreach (var child in children)
    {
        if (child.name == boneName)
            return child;
    }
    return null;
}

public void ExecuteCommandsOn(GameObject target)
{
    foreach (var command in spellCommands)
    {
        if (string.IsNullOrWhiteSpace(command.componentName) || string.IsNullOrWhiteSpace(command.methodName))
            continue;

        var component = target.GetComponent(command.componentName);
        if (component == null)
        {
            Debug.LogWarning($"[SpellCommand] Component '{command.componentName}' not found on {target.name}.");
            continue;
        }

        var method = component.GetType().GetMethod(command.methodName);
        if (method == null)
        {
            Debug.LogWarning($"[SpellCommand] Method '{command.methodName}' not found in '{command.componentName}' on {target.name}.");
            continue;
        }

        method.Invoke(component, null);
        Debug.Log($"[SpellCommand] Executed '{command.methodName}' on '{command.componentName}' in {target.name}.");
    }
}



}
