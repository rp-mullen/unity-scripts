using UnityEngine;
using System.Collections.Generic;

public class TerrainFootstepDetector : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource stepSound1; // First audio source
    public AudioSource stepSound2; // Second audio source
    private bool useFirstSource = true; // Toggles between audio sources

    [SerializeField]
    public List<TerrainSound> terrainSoundMap = new List<TerrainSound>();

    [Header("Animation Settings")]
    public Animator playerAnimator;

    private string currentTerrainType = "Default";
    private PlayerAnimatorController thirdPersonMovement;

    void Start()
    {
        thirdPersonMovement = GetComponent<PlayerAnimatorController>();
        UpdateCurrentTerrainType();
    }

    void Update()
    {
        UpdateCurrentTerrainType();
    }

    void UpdateCurrentTerrainType()
    {
        if (IsOnTerrainType(out string terrainType))
        {
            currentTerrainType = terrainType;
        }
        else
        {
            currentTerrainType = "Default"; // Fallback if no terrain detected
        }
    }

    public void TriggerFootstep()
    {
        PlayFootstepSound(currentTerrainType);
    }

    bool IsOnTerrainType(out string terrainType)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out hit, 1.5f))
        {
            Terrain terrain = hit.collider.GetComponent<Terrain>();
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                Vector3 terrainPos = terrain.transform.position;

                Vector3 localPos = hit.point - terrainPos;
                float x = localPos.x / terrainData.size.x;
                float z = localPos.z / terrainData.size.z;

                int mapX = Mathf.FloorToInt(x * terrainData.alphamapWidth);
                int mapZ = Mathf.FloorToInt(z * terrainData.alphamapHeight);

                mapX = Mathf.Clamp(mapX, 0, terrainData.alphamapWidth - 1);
                mapZ = Mathf.Clamp(mapZ, 0, terrainData.alphamapHeight - 1);

                float[,,] splatmap = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);
                string detectedTerrain = GetTerrainType(terrainData, splatmap);

                if (!string.IsNullOrEmpty(detectedTerrain))
                {
                    terrainType = detectedTerrain;
                    return true;
                }
            }
        }
        terrainType = null;
        return false;
    }

    string GetTerrainType(TerrainData terrainData, float[,,] splatmap)
    {
        for (int i = 0; i < terrainData.terrainLayers.Length; i++)
        {
            string layerName = terrainData.terrainLayers[i].name.ToLower();
            float layerAmount = splatmap[0, 0, i];

            if (layerAmount > 0.5f)
            {
                if (layerName.Contains("grass")) return "Grass";
                if (layerName.Contains("dirt") || layerName.Contains("rock")) return "Dirt";
                if (layerName.Contains("stone")) return "Stone";
            }
        }
        return null;
    }

    void PlayFootstepSound(string terrainType)
    {
        if (terrainSoundMap.Exists(mapping => mapping.terrainType == terrainType))
        {
            TerrainSound terrainMapping = terrainSoundMap.Find(mapping => mapping.terrainType == terrainType);
            if (terrainMapping.footstepSounds.Count > 0)
            {
                AudioClip clip = terrainMapping.footstepSounds[Random.Range(0, terrainMapping.footstepSounds.Count)];

                if (useFirstSource)
                {
                    stepSound1.PlayOneShot(clip);
                }
                else
                {
                    stepSound2.PlayOneShot(clip);
                }

                useFirstSource = !useFirstSource; // Toggle between sources
            }
        }
    }

    [System.Serializable]
    public struct TerrainSound
    {
        public string terrainType;
        public List<AudioClip> footstepSounds;
    }
}
