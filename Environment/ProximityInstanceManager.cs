using System.Collections.Generic;
using UnityEngine;

public class ProximityInstancer : MonoBehaviour
   {
   public InstanceMeshLibrary meshLibrary;
   public GameObject prefab;
   public Material instanceMaterial;
   public float activationRadius = 20f;
   public List<InstanceData> spawnData = new();

   private Dictionary<Vector3, GameObject> active = new();
   private HashSet<Vector3> destroyed = new();
   private Mesh bakedMesh;

   void Start()
      {
      bakedMesh = meshLibrary.GetMeshForPrefab(prefab);
      if (!bakedMesh)
         Debug.LogError("Baked mesh not found for: " + prefab.name);
      }

   void Update()
      {
      var player = GameObject.FindGameObjectWithTag("Player")?.transform;
      if (!player) return;

      Vector3 playerPos = player.position;

      foreach (var data in spawnData)
         {
         if (destroyed.Contains(data.position)) continue;

         float dist = Vector3.Distance(playerPos, data.position);
         bool isActive = active.ContainsKey(data.position);

         if (dist < activationRadius)
            {
            if (!isActive)
               {
               var go = Instantiate(prefab, data.position, data.rotation);
               go.transform.localScale = data.scale;

               var npc = go.GetComponent<NPC>();
               if (npc)
                  npc.healthComponent.OnDepleted += () => destroyed.Add(data.position);

               active[data.position] = go;
               }
            }
         else if (isActive)
            {
            Destroy(active[data.position]);
            active.Remove(data.position);
            }
         }
      }

   void OnRenderObject()
      {
      if (!bakedMesh || !instanceMaterial) return;

      List<Matrix4x4> matrices = new();

      foreach (var data in spawnData)
         {
         if (!active.ContainsKey(data.position) && !destroyed.Contains(data.position))
            matrices.Add(data.ToMatrix());
         }

      for (int i = 0; i < matrices.Count; i += 1023)
         {
         Graphics.DrawMeshInstanced(
             bakedMesh, 0, instanceMaterial,
             matrices.GetRange(i, Mathf.Min(1023, matrices.Count - i))
         );
         }
      }
   }


[System.Serializable]
public struct InstanceData
   {
   public Vector3 position;
   public Quaternion rotation;
   public Vector3 scale;

   public Matrix4x4 ToMatrix() => Matrix4x4.TRS(position, rotation, scale);
   }
