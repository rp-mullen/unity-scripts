using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using UnityEngine;

namespace Assets.Scripts.Systems.Items
   {

   public enum PlacementMode
      {
      THIRD_PERSON,
      OMNI
      }

   public class ItemPlacement : MonoBehaviour
      {

      public PlaceableItem currentItem;
      public GameObject instancePrefab;
      public Transform placementPoint;
      public float placementDistance = 2f;
      public LayerMask placementLayerMask;

      public PlacementMode placementMode = PlacementMode.THIRD_PERSON;

      public bool IsActiveItemPlaceable
         {
         get
            {
            return currentItem != null && currentItem.placementPrefab != null;
            }
         }

      public void Start()
         {

         }

      public void Update()
         {
         var playerState = GetComponent<PlayerStateMachine>();
         if (playerState == null)
            {
            Debug.LogError("PlayerStateMachine component not found on the GameObject.");
            return;
            }

         if (playerState.currState == PlayerState.Idle && IsActiveItemPlaceable)
            {
            UpdatePlacementPoint();
            if (instancePrefab != null)
               {
               if (currentItem != null)
                  {
                  UpdateItemPreview();
                  }
               }
            else
               {
               ShowItemPreview();
               }
            }
         else
            {
            if (instancePrefab != null)
               {
               Destroy(instancePrefab);
               instancePrefab = null;
               }
            }
         if (Input.GetMouseButtonDown(0) && playerState.CanPlace)
            {
            PlaceItem();
            }

         }

      public void ShowItemPreview()
         {
         if (currentItem != null)
            {
            var instance = Instantiate(currentItem.placementPrefab, placementPoint.position, placementPoint.rotation);
            instance.GetComponent<Renderer>().material = new Material(Shader.Find("PlacementPreview"));
            instance.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.5f);
            }
         }

      public void UpdateItemPreview()
         {
         if (currentItem != null)
            {
            if (IsPlaceable())
               {
               // Green
               instancePrefab.GetComponent<Renderer>().material.color = new Color(0, 1, 0, 0.5f);
               }
            else
               {
               // Red
               instancePrefab.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.5f);
               }
            }
         }


      public void PlaceItem()
         {

         WriteToPersistentStorage();
         }

      public void WriteToPersistentStorage()
         {
         if (currentItem != null)
            {
            // Save the item placement data to persistent storage
            // This could be a file, database, or any other storage mechanism
            // Example: PlayerPrefs.SetString("ItemPlacement", JsonUtility.ToJson(currentItem));
            }
         }

      public bool IsPlaceable()
         {
         bool result = false;

         Terrain terrain = Terrain.activeTerrain;
         TerrainData terrainData = terrain.terrainData;

         Vector3 worldPos = placementPoint.position;
         Vector3 terrainPos = worldPos - terrain.transform.position;

         // Normalize to terrain's size
         float normX = terrainPos.x / terrainData.size.x;
         float normZ = terrainPos.z / terrainData.size.z;

         // Steepness is returned in degrees (0° = flat, 90° = vertical)
         float steepness = terrainData.GetSteepness(normX, normZ);

         if (steepness < 45f)
            {
            result = true;
            }
         else
            {
            Debug.Log("Cannot place item on steep terrain.");
            }

         return result;
         }

      public void UpdatePlacementPoint()
         {
         if (placementMode == PlacementMode.THIRD_PERSON)
            {
            placementPoint.position = transform.position + transform.forward * placementDistance;
            Ray ray = new Ray(placementPoint.position, -transform.up);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, placementLayerMask))
               {
               Vector3 placementPosition = hit.point + hit.normal * placementDistance;
               placementPoint.position = placementPosition;
               }
            else
               {
               Debug.Log("No valid placement point found.");
               }
            }
         else if (placementMode == PlacementMode.OMNI)
            {
            // Cast a ray from the camera to the mouse position

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, placementLayerMask))
               {
               Vector3 placementPosition = hit.point + hit.normal * placementDistance;
               placementPoint.position = placementPosition;
               }
            else
               {
               Debug.Log("No valid placement point found.");
               }

            }
         }

      }
   }
