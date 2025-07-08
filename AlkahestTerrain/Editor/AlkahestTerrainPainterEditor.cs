using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(AlkahestTerrain))]
public class AlkahestTerrainPainterEditor : Editor
   {
   private AlkahestTerrain terrain;
   private AlkahestTerrainBrushLibrary brushLibrary;

   private Material brushPainterMaterial;
   private Material flowPainterMaterial;
   private Stack<AlkahestPaintCommand> undoStack = new Stack<AlkahestPaintCommand>();
   private const int MaxUndoSteps = 10;

   private int selectedLayer = 0;
   private int selectedBrushIndex = 0;

   private float brushSize = 5f;
   private float brushStrength = 0.5f;
   private float brushHardness = 1.0f;

   private float brushRotation = 0f;
   private bool autoRotate = false;
   private Vector2? lastPaintUV = null;

   private enum PaintMode { Splatmap, Flowmap, Both }
   private PaintMode paintMode = PaintMode.Splatmap;


   private void OnEnable()
      {
      terrain = (AlkahestTerrain)target;

      Shader brushShader = Shader.Find("Hidden/Alkahest/BrushPainter");
      if (brushShader != null)
         brushPainterMaterial = new Material(brushShader);
      else
         Debug.LogError("BrushPainter shader not found!");

      Shader flowShader = Shader.Find("Hidden/Alkahest/FlowPainter");
      if (flowShader != null)
         flowPainterMaterial = new Material(flowShader);
      else
         Debug.LogError("FlowPainter shader not found!");


      if (brushLibrary == null)
         brushLibrary = Resources.Load<AlkahestTerrainBrushLibrary>("DefaultBrushLibrary");
      }

   public override void OnInspectorGUI()
      {
      DrawDefaultInspector();

      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Alkahest Terrain Painter", EditorStyles.boldLabel);

      paintMode = (PaintMode)EditorGUILayout.EnumPopup("Paint Mode", paintMode);


      selectedLayer = GUILayout.SelectionGrid(selectedLayer, new[] { "Layer 0 (R)", "Layer 1 (G)", "Layer 2 (B)", "Layer 3 (A)" }, 2);
      brushSize = EditorGUILayout.Slider("Brush Size", brushSize, 0.1f, 50f);
      brushStrength = EditorGUILayout.Slider("Brush Strength", brushStrength, 0.01f, 1f);
      brushHardness = EditorGUILayout.Slider("Brush Hardness", brushHardness, 0.1f, 4f);

      EditorGUILayout.Space();
      EditorGUILayout.LabelField("Brush Rotation", EditorStyles.boldLabel);

      autoRotate = EditorGUILayout.Toggle("Auto-Rotate", autoRotate);

      if (!autoRotate)
         brushRotation = EditorGUILayout.Slider("Manual Rotation", brushRotation, 0f, 360f);

      EditorGUILayout.Space();
      brushLibrary = (AlkahestTerrainBrushLibrary)EditorGUILayout.ObjectField("Brush Library", brushLibrary, typeof(AlkahestTerrainBrushLibrary), false);

      if (brushLibrary != null && brushLibrary.brushes.Count > 0)
         {
         EditorGUILayout.LabelField("Brushes:");

         int brushCols = 4;
         int rows = Mathf.CeilToInt(brushLibrary.brushes.Count / (float)brushCols);
         selectedBrushIndex = GUILayout.SelectionGrid(selectedBrushIndex, GetBrushGUIContents(), brushCols, GUILayout.Height(rows * 64));
         }

      if (GUILayout.Button("Undo Last Paint"))
         UndoLastPaint();

      EditorGUILayout.HelpBox("Hold CTRL + Left Click to paint in Scene view.\nAuto-Rotate aligns brush to stroke direction.", MessageType.Info);
      }

   private GUIContent[] GetBrushGUIContents()
      {
      if (brushLibrary == null || brushLibrary.brushes == null)
         return new GUIContent[0];

      GUIContent[] contents = new GUIContent[brushLibrary.brushes.Count];
      for (int i = 0; i < brushLibrary.brushes.Count; i++)
         {
         Texture2D tex = brushLibrary.brushes[i];
         contents[i] = new GUIContent(tex != null ? tex : Texture2D.whiteTexture);
         }
      return contents;
      }

   private Texture2D GetSelectedBrushTexture()
      {
      if (brushLibrary == null || brushLibrary.brushes == null || brushLibrary.brushes.Count == 0)
         return null;

      if (selectedBrushIndex < 0 || selectedBrushIndex >= brushLibrary.brushes.Count)
         return null;

      return brushLibrary.brushes[selectedBrushIndex];
      }

   private void OnSceneGUI()
      {
      Event e = Event.current;
    Debug.Log($"OnSceneGUI event: {e.type}, control: {e.control}");

      if (e.control && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
         {
         Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
         if (Physics.Raycast(ray, out RaycastHit hit))
            {
            if (hit.collider.gameObject == terrain.gameObject)
               {
               UpdateBrushRotation(hit.textureCoord);
               RecordUndo();
               if (paintMode == PaintMode.Splatmap || paintMode == PaintMode.Both)
                  PaintAtPointGPU(hit.textureCoord);

               if (paintMode == PaintMode.Flowmap || paintMode == PaintMode.Both)
                  PaintFlowMapAtPoint(hit.textureCoord);

               e.Use();
               }
            }
         }

      if (e.type == EventType.MouseUp)
         lastPaintUV = null;
      }

   private void UpdateBrushRotation(Vector2 currentUV)
      {
      if (autoRotate && lastPaintUV.HasValue)
         {
         Vector2 delta = currentUV - lastPaintUV.Value;
         if (delta.sqrMagnitude > 0.0001f)
            brushRotation = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
         }

      lastPaintUV = currentUV;
      }

   private void RecordUndo()
      {
      if (terrain.controlSplatmap != null)
         {
         undoStack.Push(new AlkahestPaintCommand(terrain.controlSplatmap));
         if (undoStack.Count > MaxUndoSteps)
            undoStack.Pop();
         }
      }

   private void UndoLastPaint()
      {
      if (undoStack.Count > 0)
         {
         AlkahestPaintCommand cmd = undoStack.Pop();
         cmd.Restore(terrain.controlSplatmap);
         SceneView.RepaintAll();
         }
      }

   private void PaintAtPointGPU(Vector2 uv)
      {
      if (terrain.controlSplatmap == null || brushPainterMaterial == null)
         {
         Debug.LogWarning("Cannot paint: missing splatmap or brush material.");
         return;
         }

      int resolution = terrain.controlSplatmap.width;

      Texture2D brushTex = GetSelectedBrushTexture();
      if (brushTex == null)
         brushTex = BrushUtils.GenerateCircleBrush(resolution, uv, brushSize / resolution, brushHardness);

      RenderTexture brushMaskRT = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.R8);
      Graphics.Blit(brushTex, brushMaskRT);

      brushPainterMaterial.SetTexture("_MainTex", terrain.controlSplatmap);
      brushPainterMaterial.SetTexture("_BrushMask", brushMaskRT);
      brushPainterMaterial.SetFloat("_LayerIndex", selectedLayer);
      brushPainterMaterial.SetFloat("_BrushStrength", brushStrength);
      brushPainterMaterial.SetFloat("_BrushRotation", brushRotation);

      RenderTexture temp = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.ARGB32);

      Graphics.Blit(terrain.controlSplatmap, temp);
      Graphics.Blit(temp, terrain.controlSplatmap, brushPainterMaterial);

      RenderTexture.ReleaseTemporary(temp);
      RenderTexture.ReleaseTemporary(brushMaskRT);

      if (GetSelectedBrushTexture() == null)
         DestroyImmediate(brushTex);

      SceneView.RepaintAll();
      }

   private void PaintFlowMapAtPoint(Vector2 uv)
      {
      if (terrain.flowMap == null || brushPainterMaterial == null)
         {
         Debug.LogWarning("Cannot paint flow: missing flow map or brush material.");
         return;
         }

      int resolution = terrain.flowMap.width;

      // Get brush texture or fallback to procedural
      Texture2D brushTex = GetSelectedBrushTexture();
      if (brushTex == null)
         {
         brushTex = BrushUtils.GenerateCircleBrush(resolution, uv, brushSize / resolution, brushHardness);
         }

      // Create temporary brush mask RT
      RenderTexture brushMaskRT = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.R8);
      Graphics.Blit(brushTex, brushMaskRT);

      // Normalize brushRotation (0–360) to 0–1
      float normalizedRotation = brushRotation / 360f;
      if (normalizedRotation < 0f) normalizedRotation += 1f;  // ensure positive

      flowPainterMaterial.SetTexture("_MainTex", terrain.flowMap);
      flowPainterMaterial.SetTexture("_BrushMask", brushMaskRT);
      flowPainterMaterial.SetFloat("_BrushRotation", normalizedRotation);

      RenderTexture temp = RenderTexture.GetTemporary(resolution, resolution, 0, RenderTextureFormat.R8);
      Graphics.Blit(terrain.flowMap, temp, flowPainterMaterial);
      Graphics.Blit(temp, terrain.flowMap);


      RenderTexture.ReleaseTemporary(temp);
      RenderTexture.ReleaseTemporary(brushMaskRT);

      if (GetSelectedBrushTexture() == null)
         DestroyImmediate(brushTex);  // Only destroy if procedural

      SceneView.RepaintAll();
      }

   }
