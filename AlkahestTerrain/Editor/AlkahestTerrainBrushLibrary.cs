using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AlkahestBrushLibrary", menuName = "Alkahest Terrain/Brush Library")]
public class AlkahestTerrainBrushLibrary : ScriptableObject
   {
   public List<Texture2D> brushes = new List<Texture2D>();
   }
