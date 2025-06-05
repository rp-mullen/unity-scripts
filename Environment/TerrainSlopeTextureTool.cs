using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class SlopeTexturePainter : MonoBehaviour
{
    [Range(0f, 90f)]
    public float slopeThreshold = 25f;

    [Tooltip("Index of the texture in the terrain's layer list to apply to steep slopes")]
    public int steepTextureIndex = 2;

    [ContextMenu("Apply Slope Texture")]
    public void ApplySlopeTexture()
    {
        Terrain terrain = GetComponent<Terrain>();
        if (terrain == null || terrain.terrainData == null)
        {
            Debug.LogError("No Terrain or TerrainData found.");
            return;
        }

        TerrainData data = terrain.terrainData;
        int res = data.alphamapResolution;
        float[,,] alphamaps = data.GetAlphamaps(0, 0, res, res);
        int layers = data.alphamapLayers;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float normX = (float)x / (res - 1);
                float normY = (float)y / (res - 1);

                Vector3 normal = data.GetInterpolatedNormal(normX, normY);
                float slope = Vector3.Angle(normal, Vector3.up);

                float[] weights = new float[layers];

                if (slope >= slopeThreshold)
                {
                    weights[steepTextureIndex] = 1f;
                }
                else
                {
                    // Preserve original blend (optional: normalize or bias here if needed)
                    for (int i = 0; i < layers; i++)
                        weights[i] = alphamaps[y, x, i];
                }

                // Normalize
                float total = 0f;
                foreach (float w in weights) total += w;
                for (int i = 0; i < layers; i++) weights[i] /= total;

                for (int i = 0; i < layers; i++)
                    alphamaps[y, x, i] = weights[i];
            }
        }

        data.SetAlphamaps(0, 0, alphamaps);
        Debug.Log("Slope texture painting complete.");
    }
}
