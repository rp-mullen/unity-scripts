using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Systems.Magic
{
   [System.Serializable]
   public class MagicalState
      {
      [Header("Elementality")]
      public float fire;
      public float water;
      public float earth;
      public float air;

      public List<float> elementality => new List<float> { fire, water, earth, air };

      [Header("Vitality")]
      public float mind;
      public float essence;

      public List<float> vitality => new List<float> { mind, essence };

      [Header("Resonances")]
      // magical conductivity
      public List<float> resonances = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f };

      public void calculateFromStats(CharacterStats characterStats)
         {
         // Umbrella model (each stat contributes to 2 axes)
         fire = (characterStats.Str + characterStats.Cha) / 2f;
         water = (characterStats.Wis + characterStats.Int) / 2f;
         earth = (characterStats.Con + characterStats.Str) / 2f;
         air = (characterStats.Dex + characterStats.Cha) / 2f;

         mind = (characterStats.Int + characterStats.Wis) / 2f;
         essence = (characterStats.Wis + characterStats.Con) / 2f;

         }


      }

   public class Signature : MonoBehaviour
    {
      
      public Action OnExcited;
      public MagicalState baseState;
      public MagicalState currentState;
      public bool isExcited = false;

      public void Start()
         {
         // Initialize resonances if needed
         }

      public void Excite(List<float> deltas)
         {

         }
      }
}
