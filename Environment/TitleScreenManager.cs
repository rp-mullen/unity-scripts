using UnityEngine;
using Unity.Netcode;

public class TitleScreenManager : MonoBehaviour
{
   public void startNetworkAsHost() {
    NetworkManager.Singleton.StartHost();
   }

   public void StartNewGame() 
   {
      StartCoroutine(WorldSaveGameManager.instance.LoadNewGame());
   }
}
