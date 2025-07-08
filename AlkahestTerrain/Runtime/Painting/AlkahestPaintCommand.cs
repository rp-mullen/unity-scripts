using UnityEngine;

public class AlkahestPaintCommand
   {
   public Texture2D previousState;

   public AlkahestPaintCommand(RenderTexture splatmap)
      {
      previousState = new Texture2D(splatmap.width, splatmap.height, TextureFormat.RGBA32, false);
      RenderTexture.active = splatmap;
      previousState.ReadPixels(new Rect(0, 0, splatmap.width, splatmap.height), 0, 0);
      previousState.Apply();
      RenderTexture.active = null;
      }

   public void Restore(RenderTexture target)
      {
      Graphics.Blit(previousState, target);
      }
   }
