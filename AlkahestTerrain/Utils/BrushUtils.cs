using UnityEngine;

public static class BrushUtils
   {
   public static Texture2D GenerateCircleBrush(int size, Vector2 uv, float brushRadius01, float hardness = 1.0f)
      {
      Texture2D tex = new Texture2D(size, size, TextureFormat.R8, false);
      tex.wrapMode = TextureWrapMode.Clamp;

      int centerX = Mathf.RoundToInt(uv.x * size);
      int centerY = Mathf.RoundToInt(uv.y * size);
      int radiusPixels = Mathf.RoundToInt(brushRadius01 * size);

      for (int y = 0; y < size; y++)
         {
         for (int x = 0; x < size; x++)
            {
            float dx = x - centerX;
            float dy = y - centerY;
            float dist = Mathf.Sqrt(dx * dx + dy * dy);
            float normDist = Mathf.Clamp01(dist / radiusPixels);

            // Smooth falloff using hardness control
            float strength = Mathf.SmoothStep(1f, 0f, normDist);
            strength = Mathf.Pow(strength, hardness); // optional sharpness

            tex.SetPixel(x, y, new Color(strength, strength, strength));
            }
         }

      tex.Apply();
      return tex;
      }
   }
