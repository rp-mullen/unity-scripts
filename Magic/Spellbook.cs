using UnityEngine;
using System.Collections.Generic;


public class Spellbook : MonoBehaviour
{
    public List<Spell> knownSpells = new List<Spell>();
    public List<Spell> equippedSpells = new List<Spell>();
    public int maxEquippedSpells = 5;

    public bool LearnSpell(Spell newSpell)
    {
        if (!knownSpells.Contains(newSpell))
        {
            knownSpells.Add(newSpell);
            return true;
        }
        return false;
    }

    public bool EquipSpell(Spell spell)
    {
        if (knownSpells.Contains(spell) && !equippedSpells.Contains(spell))
        {
            if (equippedSpells.Count < maxEquippedSpells)
            {
                equippedSpells.Add(spell);
                return true;
            }
        }
        return false;
    }

    public void UnequipSpell(Spell spell)
    {
        equippedSpells.Remove(spell);
    }
}
