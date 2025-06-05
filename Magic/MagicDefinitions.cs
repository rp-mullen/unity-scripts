using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Describes the purpose of a spell (e.g. damage, healing, buffing).
/// </summary>
public enum SpellType
{
    Damage,
    Heal,
    Buff,
    Debuff,
    Utility
}

/// <summary>
/// Represents the traditional magical schools from fantasy paradigms.
/// </summary>
public enum MagicSchool
{
    Evocation,
    Abjuration,
    Conjuration,
    Divination,
    Enchantment,
    Illusion,
    Transmutation,
    Necromancy
}

/// <summary>
/// The geometric shape the spell affects.
/// </summary>
public enum SpellShape
{
    Point,
    Cone,
    Line,
    Circle,
    Sphere,
    Rectangle,
    Cube
}

/// <summary>
/// Target classification system for spell application.
/// </summary>
public enum SpellTargeting
{
    Enemy,
    Ally,
    Ground,
    Self,
    Area,
    Object
}

/// <summary>
/// Types of magical elements used in spell composition.
/// </summary>
public enum SpellElementType
{
    Fire,
    Water,
    Earth,
    Air,
    Ice,
    Steam,
    Blood,
    Mud,
    Poison,
    Lightning,
    Force,
    Life,
    Necrotic,
    Mind,
    None
}

/// <summary>
/// A timed effect applied by a spell that alters the state of the target.
/// </summary>
[Serializable]
public class StatusEffect
{
    public string effectName;
    public float magnitude;
    public float duration;
}

/// <summary>
/// Represents a callable command component in a spell.
/// </summary>
[Serializable]
public class SpellCommand
{
    public string componentName;
    public string methodName;
}

/// <summary>
/// Represents a magical field state at a specific location or object.
/// </summary>
[Serializable]
public class ManaFieldState
{
    public ElementVector elementVector = new();
    public float vitality = 0f; // > 0 = life, < 0 = undeath
    public MotionVector motionVector = new();
    public string boundSigil;

    public SpellElementType GetDominantElement() => elementVector.GetDominantElement();

    public bool IsLiving => vitality > 0;
    public bool IsUndead => vitality < 0;
    public bool IsInert => vitality == 0;
}

/// <summary>
/// Represents the elemental makeup of a state. Each value represents an excitation level of the elemental field.
/// </summary>
[Serializable]
public class ElementVector
{
    public float Fire;
    public float Water;
    public float Earth;
    public float Air;

    public List<float> Values => new() { Fire, Water, Earth, Air };

    /// <summary>
    /// Returns the element with the highest excitation value.
    /// </summary>
    public SpellElementType GetDominantElement()
    {
        float max = float.NegativeInfinity;
        SpellElementType dominant = SpellElementType.None;

        foreach (SpellElementType type in Enum.GetValues(typeof(SpellElementType)))
        {
            float val = type switch
            {
                SpellElementType.Fire => Fire,
                SpellElementType.Water => Water,
                SpellElementType.Earth => Earth,
                SpellElementType.Air => Air,
                _ => 0f
            };

            if (val > max)
            {
                max = val;
                dominant = type;
            }
        }

        return dominant;
    }

    /// <summary>
    /// Combines two elemental types into a compound type, if a known result exists.
    /// </summary>
    public static SpellElementType Combine(SpellElementType a, SpellElementType b)
    {
        if (CommuteCompare(a, b, SpellElementType.Fire, SpellElementType.Water)) return SpellElementType.Steam;
        if (CommuteCompare(a, b, SpellElementType.Earth, SpellElementType.Water)) return SpellElementType.Mud;
        if (CommuteCompare(a, b, SpellElementType.Earth, SpellElementType.Air)) return SpellElementType.Lightning;
        if (CommuteCompare(a, b, SpellElementType.Water, SpellElementType.Necrotic)) return SpellElementType.Poison;
        return SpellElementType.None;
    }

    /// <summary>
    /// Attempts to reverse-combine a compound element into its components.
    /// </summary>
    public static (SpellElementType, SpellElementType)? Decompose(SpellElementType type) => type switch
    {
        SpellElementType.Steam => (SpellElementType.Fire, SpellElementType.Water),
        SpellElementType.Mud => (SpellElementType.Earth, SpellElementType.Water),
        SpellElementType.Lightning => (SpellElementType.Earth, SpellElementType.Air),
        SpellElementType.Poison => (SpellElementType.Water, SpellElementType.Necrotic),
        _ => null
    };

    /// <summary>
    /// Returns true if a and b match types t1 and t2 in any order.
    /// </summary>
    private static bool CommuteCompare(SpellElementType a, SpellElementType b, SpellElementType t1, SpellElementType t2) =>
        (a == t1 && b == t2) || (a == t2 && b == t1);

    public static bool CommuteCompare<T>(T t1, T t2, T a, T b) where T : Enum =>
        (EqualityComparer<T>.Default.Equals(a, t1) && EqualityComparer<T>.Default.Equals(b, t2)) ||
        (EqualityComparer<T>.Default.Equals(a, t2) && EqualityComparer<T>.Default.Equals(b, t1));
}

/// <summary>
/// Stores amplification factors for each elemental type. Used to modify spell power based on materials or fields.
/// </summary>
[Serializable]
public class ElementAmplification
{
    public float Fire = 1.0f;
    public float Water = 1.0f;
    public float Earth = 1.0f;
    public float Air = 1.0f;
    public float Life = 1.0f;
    public float Necrotic = 1.0f;
    public float Force = 1.0f;
    public float Mind = 1.0f;

    /// <summary>
    /// Retrieves amplification for the given element.
    /// </summary>
    public float GetAmplification(SpellElementType type) => type switch
    {
        SpellElementType.Fire => Fire,
        SpellElementType.Water => Water,
        SpellElementType.Earth => Earth,
        SpellElementType.Air => Air,
        SpellElementType.Life => Life,
        SpellElementType.Necrotic => Necrotic,
        SpellElementType.Force => Force,
        SpellElementType.Mind => Mind,
        _ => 1.0f
    };

    /// <summary>
    /// Sets amplification factor for a specific element.
    /// </summary>
    public void SetAmplification(SpellElementType type, float value)
    {
        switch (type)
        {
            case SpellElementType.Fire: Fire = value; break;
            case SpellElementType.Water: Water = value; break;
            case SpellElementType.Earth: Earth = value; break;
            case SpellElementType.Air: Air = value; break;
            case SpellElementType.Life: Life = value; break;
            case SpellElementType.Necrotic: Necrotic = value; break;
            case SpellElementType.Force: Force = value; break;
            case SpellElementType.Mind: Mind = value; break;
        }
    }
}

/// <summary>
/// Represents motion-related components of a spell field (shape, vector, intensity).
/// </summary>
[Serializable]
public class MotionVector
{
    public SpellShape shape;
    public Vector3 origin;
    public Vector3 direction;
    public float intensity;
}
