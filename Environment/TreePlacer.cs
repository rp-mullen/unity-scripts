using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TerrainGrassTreeSpawner : MonoBehaviour
{
    [Header("Tree Spawning Settings")]
    public List<GameObject> treePrefabs;
    [Range(0f, 90f)] public float slopeCutoff = 30f;
    [Min(1)] public int instanceCount = 200; // Fraction of matching points to spawn trees

    [Tooltip("Indices of terrain layers considered 'grass'")]
    public List<int> grassTextureIndices;

    [Header("Scale Randomization")]
    public float minScale = 0.8f;
    public float maxScale = 1.5f;

    [ContextMenu("Spawn Trees on Grass")]
    [ContextMenu("Spawn Trees on Grass")]
    [ContextMenu("Spawn Trees on Grass")]
    public void SpawnTrees()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("No Terrain or TerrainData found.");
            return;
        }

        TerrainData data = terrain.terrainData;
        Vector3 terrainOrigin = terrain.GetPosition();
        int resolution = data.alphamapResolution;
        float[,,] alphamaps = data.GetAlphamaps(0, 0, resolution, resolution);
        float terrainWidth = data.size.x;
        float terrainLength = data.size.z;
        int layers = data.alphamapLayers;

        // Setup parent
        Transform parent = new GameObject("SpawnedTrees").transform;
        parent.SetParent(transform);
        parent.position = terrainOrigin;

        int spawnCount = 0;
        int attempts = 0;
        int maxAttempts = instanceCount * 10;

        while (spawnCount < instanceCount && attempts < maxAttempts)
        {
            attempts++;

            float normX = Random.value;
            float normY = Random.value;

            int x = Mathf.RoundToInt(normX * (resolution - 1));
            int y = Mathf.RoundToInt(normY * (resolution - 1));

            // Check if grass is dominant
            bool isGrass = false;
            foreach (int idx in grassTextureIndices)
            {
                if (idx < layers && alphamaps[y, x, idx] > 0.5f)
                {
                    isGrass = true;
                    break;
                }
            }

            if (!isGrass)
                continue;

            // Calculate world position
            float worldX = terrainOrigin.x + normX * terrainWidth;
            float worldZ = terrainOrigin.z + normY * terrainLength;
            Vector3 worldPos = new Vector3(worldX, 0f, worldZ);
            worldPos.y = terrain.SampleHeight(worldPos) + terrainOrigin.y;

            // Check slope
            Vector3 normal = data.GetInterpolatedNormal(normX, normY);
            float slope = Vector3.Angle(normal, Vector3.up);
            if (slope > slopeCutoff)
                continue;

            GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Count)];
            if (prefab == null) continue;

            GameObject instance;
#if UNITY_EDITOR
            var type = PrefabUtility.GetPrefabAssetType(prefab);
            if (type == PrefabAssetType.NotAPrefab || type == PrefabAssetType.MissingAsset)
                instance = Instantiate(prefab);
            else
                instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
#else
        instance = Instantiate(prefab);
#endif

            if (instance == null) continue;

            instance.transform.SetPositionAndRotation(worldPos, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

            float scale = Random.Range(minScale, maxScale);
            Vector3 originalScale = instance.transform.localScale;
            instance.transform.localScale = new Vector3(
                originalScale.x * scale * 0.7f,
                originalScale.y * scale,
                originalScale.z * scale * 0.7f
            );

            instance.transform.eulerAngles = new Vector3(-90f, Random.RandomRange(0f, 180f), 0f);

            instance.GetComponentInChildren<SkinnedMeshRenderer>().SetBlendShapeWeight(0, Random.RandomRange(0f, 100f));

            instance.transform.SetParent(parent, true);

            spawnCount++;
        }

        Debug.Log($"Spawned {spawnCount} trees (attempted {attempts}).");
    }

}
