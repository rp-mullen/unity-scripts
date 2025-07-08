using UnityEditor;
using UnityEngine;

public class AlkahestTerrainPainterWindow : EditorWindow
{
    private AlkahestTerrain targetTerrain;

    private int selectedLayer = 0;
    private float brushSize = 5f;
    private float brushStrength = 0.5f;
    private float brushHardness = 1f;
    private float brushRotation = 0f;
    private bool autoRotate = false;

    [MenuItem("Window/Alkahest Terrain Painter")]
    public static void ShowWindow()
    {
        GetWindow<AlkahestTerrainPainterWindow>("Alkahest Painter");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Alkahest Terrain Painter", EditorStyles.boldLabel);

        targetTerrain = (AlkahestTerrain)EditorGUILayout.ObjectField("Target Terrain", targetTerrain, typeof(AlkahestTerrain), true);

        EditorGUILayout.Space();

        selectedLayer = GUILayout.SelectionGrid(selectedLayer, new[] { "Layer 0 (R)", "Layer 1 (G)", "Layer 2 (B)", "Layer 3 (A)" }, 2);

        brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 50f);
        brushStrength = EditorGUILayout.Slider("Brush Strength", brushStrength, 0.01f, 1f);
        brushHardness = EditorGUILayout.Slider("Brush Hardness", brushHardness, 0.1f, 4f);

        autoRotate = EditorGUILayout.Toggle("Auto Rotate", autoRotate);
        if (!autoRotate)
            brushRotation = EditorGUILayout.Slider("Brush Rotation", brushRotation, 0f, 360f);

        EditorGUILayout.Space();

        if (GUILayout.Button("Paint at SceneView Click"))
        {
            SceneView.duringSceneGui += OnSceneGUI;
            Debug.Log("Click anywhere in SceneView to paint...");
        }

        if (GUILayout.Button("Stop Painting"))
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0 && e.control)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (targetTerrain != null && hit.collider.gameObject == targetTerrain.gameObject)
                {
                    Debug.Log($"Painting at {hit.point} on {targetTerrain.gameObject.name}");
                    targetTerrain.InitializeIfNeeded();  // Ensure everything is ready

                    // Your Paint Logic Here:
                    // Example:
                    // targetTerrain.PaintAtUV(hit.textureCoord, selectedLayer, brushSize, brushStrength, brushRotation);

                    e.Use();
                }
            }
        }
    }
}
