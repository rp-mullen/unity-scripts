using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class MeshBaker
   {
   public static Mesh BakeMeshAndExtractMaterials(GameObject prefab, out List<Material> materials)
      {
      GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
      instance.hideFlags = HideFlags.HideAndDontSave;

      var renderers = instance.GetComponentsInChildren<Renderer>(true);
      Dictionary<Material, List<CombineInstance>> materialGroups = new();
      materials = new List<Material>();

      Vector3 rootPosition = instance.transform.position;

      foreach (var renderer in renderers)
         {
         Mesh mesh = null;

         if (renderer is MeshRenderer mr)
            {
            var filter = mr.GetComponent<MeshFilter>();
            if (!filter || !filter.sharedMesh) continue;
            mesh = filter.sharedMesh;
            }
         else if (renderer is SkinnedMeshRenderer smr)
            {
            if (!smr.sharedMesh) continue;
            mesh = new Mesh();
            smr.BakeMesh(mesh);
            }

         if (!mesh) continue;

         for (int i = 0; i < mesh.subMeshCount; i++)
            {
            if (i >= renderer.sharedMaterials.Length) continue;
            var material = renderer.sharedMaterials[i];
            if (!material) continue;

            if (!materialGroups.ContainsKey(material))
               {
               materialGroups[material] = new List<CombineInstance>();
               materials.Add(material);
               }

            Vector3 localPos = renderer.transform.position - rootPosition;
            Matrix4x4 localMatrix = Matrix4x4.TRS(localPos, renderer.transform.rotation, renderer.transform.lossyScale);

            materialGroups[material].Add(new CombineInstance
               {
               mesh = mesh,
               subMeshIndex = i,
               transform = localMatrix
               });
            }
         }

      if (materialGroups.Count == 0)
         {
         Debug.LogWarning("No geometry found to combine.");
         Object.DestroyImmediate(instance);
         return null;
         }

      // Combine grouped submeshes by material into separate submeshes
      List<CombineInstance> submeshBlocks = new();
      foreach (var kvp in materialGroups)
         {
         Mesh partial = new Mesh();
         partial.CombineMeshes(kvp.Value.ToArray(), true, true);

         submeshBlocks.Add(new CombineInstance
            {
            mesh = partial,
            subMeshIndex = 0,
            transform = Matrix4x4.identity
            });
         }

      Mesh final = new Mesh();
      final.name = prefab.name + "_Combined";
      final.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
      final.CombineMeshes(submeshBlocks.ToArray(), false, false); // preserve submeshes

      final.RecalculateBounds();
      final.RecalculateNormals();

      Object.DestroyImmediate(instance);
      return final;
      }
   }
