using UnityEngine;


/// <summary>
/// Attach this to any MeshRenderer + MeshCollider object to make it splatmap-paintable.
/// Automatically assigns material, creates splatmap and flowmap, and applies layer properties.
/// </summary>
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
public class AlkahestTerrain : MonoBehaviour
{
    [Header("Splatmap Data")]
    public RenderTexture controlSplatmap;

    [Header("Terrain Layers (up to 4)")]
    public Texture2D[] layerAlbedos = new Texture2D[4];
    public Texture2D[] layerNormals = new Texture2D[4];
    public Vector2[] layerTilings = new Vector2[4];

    [Header("Flow Map")]
    public RenderTexture flowMap;

    private MaterialPropertyBlock _props;
    private MeshRenderer _renderer;

    private const int MapResolution = 1024;

    private void Awake()
    {
        InitializeIfNeeded();
    }

    /// <summary>
    /// Safely ensures material, splatmap, flowmap, and property block are all ready.
    /// </summary>
    public void InitializeIfNeeded()
    {
        _renderer = GetComponent<MeshRenderer>();
        if (_props == null)
            _props = new MaterialPropertyBlock();

        EnsureMaterialAssigned();
        EnsureSplatmap();
        EnsureFlowMap();

        ApplyMaterialProperties();
    }

    private void EnsureMaterialAssigned()
    {
        if (_renderer.sharedMaterial == null)
        {
            Shader terrainShader = Shader.Find("Alkahest/TerrainBlend");
            if (terrainShader == null)
            {
                Debug.LogError("AlkahestTerrainShader.shader not found! Check your shader path and name.");
                return;
            }

            Material newMat = new Material(terrainShader)
            {
                name = "AlkahestTerrainMaterial (Auto)"
            };
            _renderer.sharedMaterial = newMat;
        }
    }

    private void EnsureSplatmap()
    {
        if (controlSplatmap == null)
        {
            controlSplatmap = new RenderTexture(MapResolution, MapResolution, 0, RenderTextureFormat.ARGB32)
            {
                name = "AlkahestSplatmap",
                enableRandomWrite = true
            };
            controlSplatmap.Create();
        }
    }

    private void EnsureFlowMap()
    {
        if (flowMap == null)
        {
            flowMap = new RenderTexture(MapResolution, MapResolution, 0, RenderTextureFormat.R8)
            {
                name = "AlkahestFlowMap",
                enableRandomWrite = true
            };
            flowMap.Create();
        }
    }

    private void ApplyMaterialProperties()
    {
        _renderer.GetPropertyBlock(_props);

        _props.SetTexture("_Splatmap", controlSplatmap);
        _props.SetTexture("_FlowMap", flowMap);
        _props.SetFloat("_FlowMapScale", MapResolution);

        for (int i = 0; i < 4; i++)
        {
            if (i < layerAlbedos.Length && layerAlbedos[i] != null)
                _props.SetTexture($"_Layer{i}Albedo", layerAlbedos[i]);

            if (i < layerNormals.Length && layerNormals[i] != null)
                _props.SetTexture($"_Layer{i}Normal", layerNormals[i]);

            if (i < layerTilings.Length)
                _props.SetVector($"_Layer{i}Tiling", new Vector4(layerTilings[i].x, layerTilings[i].y, 0, 0));
        }

        _renderer.SetPropertyBlock(_props);
    }

    public RenderTexture GetSplatmap() => controlSplatmap;
    public RenderTexture GetFlowMap() => flowMap;
}
