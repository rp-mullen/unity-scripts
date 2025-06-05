using UnityEngine;

public class AttunementComponent : MonoBehaviour
{
    [Header("Base Values")]
    public int baseAttunement = 0;          // From race or class
    public int intelligenceBonus = 0;       // From stats
    public int wisdomBonus = 0;             // Optional
    public int attunementModifiers = 0;     // Items, buffs, bloodlines, etc.

    [Header("Thresholds")]
    public int cantripThreshold = 1;
    public int spellcastingThreshold = 4;
    public int advancedMagicThreshold = 7;

    public int TotalAttunement => baseAttunement + intelligenceBonus + wisdomBonus + attunementModifiers;

    public enum AttunementTier
    {
        None,
        Faint,
        Basic,
        Advanced,
        Master
    }

    public AttunementTier GetAttunementTier()
    {
        int attune = TotalAttunement;

        if (attune < cantripThreshold)
            return AttunementTier.None;
        if (attune < spellcastingThreshold)
            return AttunementTier.Faint;
        if (attune < advancedMagicThreshold)
            return AttunementTier.Basic;

        // You can add more tiers if needed
        return AttunementTier.Advanced;
    }

    public bool CanUseCantrips() => TotalAttunement >= cantripThreshold;
    public bool CanCastBasicSpells() => TotalAttunement >= spellcastingThreshold;
    public bool CanCastAdvancedSpells() => TotalAttunement >= advancedMagicThreshold;
}
