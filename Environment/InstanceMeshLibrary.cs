using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InstanceMeshLibrary", menuName = "IML/Instance Mesh Library")]
public class InstanceMeshLibrary : ScriptableObject
   {
   [System.Serializable]
   public class Entry
      {
      public GameObject prefab;
      public string meshResourcePath;
      public Material[] materials; // assigned during bake
      }

   public List<Entry> entries = new();

   public Mesh GetMeshForPrefab(GameObject prefab)
      {
      var entry = entries.Find(e => e.prefab == prefab);
      return entry != null ? Resources.Load<Mesh>("InstanceMeshes/" + entry.meshResourcePath) : null;
      }
   }
