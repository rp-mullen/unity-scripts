#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class BakeMeshTool
   {
   [MenuItem("Tools/Instance Mesh/Bake Selected Prefab")]
   public static void BakeSelected()
      {
      GameObject prefab = Selection.activeGameObject;
      if (!prefab)
         {
         Debug.LogWarning("No prefab selected.");
         return;
         }

      Mesh mesh = MeshBaker.BakeMeshAndExtractMaterials(prefab, out var materials);
      if (!mesh)
         {
         Debug.LogWarning("Mesh baking failed.");
         return;
         }

      string meshName = prefab.name + "_Instance";
      string folderPath = "Assets/Resources/InstanceMeshes";
      string assetPath = $"{folderPath}/{meshName}.asset";

      if (!Directory.Exists(folderPath))
         Directory.CreateDirectory(folderPath);

      AssetDatabase.CreateAsset(mesh, assetPath);
      AssetDatabase.SaveAssets();
      Debug.Log($"Baked mesh saved at: {assetPath}");

      var library = Resources.Load<InstanceMeshLibrary>("InstanceMeshes/InstanceMeshLibrary");
      if (!library)
         {
         Debug.LogWarning("InstanceMeshLibrary not found in Resources/InstanceMeshes/");
         return;
         }

      var entry = library.entries.Find(e => e.prefab == prefab);
      if (entry == null)
         {
         entry = new InstanceMeshLibrary.Entry
            {
            prefab = prefab,
            meshResourcePath = meshName,
            materials = materials.ToArray()
            };
         library.entries.Add(entry);
         }
      else
         {
         entry.meshResourcePath = meshName;
         entry.materials = materials.ToArray();
         }

      EditorUtility.SetDirty(library);
      AssetDatabase.SaveAssets();
      Debug.Log($"InstanceMeshLibrary updated with entry for: {prefab.name}");
      }
   }
#endif
